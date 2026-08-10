using Database.Entities;
using Database.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service.Dto;

namespace Service;

public class NotificationDispatcherService(
    NotificationManagerService notificationManagerService,
    UserAppointmentValidationService userAppointmentValidationService,
    RoleManager<RoleEntity> roleManager,
    UserRoleService userRoleService,
    TrackedLocationForUserRepository trackedLocationForUserRepository,
    UserProfileRepository userProfileRepository,
    SeenAppointmentRepository seenAppointmentRepository,
    AppointmentLocationRepository appointmentLocationRepository,
    IConfiguration configuration,
    ILogger<NotificationDispatcherService> logger)
{
    /// <summary>
    ///     Notifies every user tracking a location about appointments they are eligible for.
    ///     Eligibility is gated by two per-role knobs: a release delay, which decides how soon after
    ///     an appointment appears the user hears about it, and a cooldown, which caps how often they
    ///     can be notified at all.
    /// </summary>
    public async Task SendNotificationForLocation(
        List<LocationAppointmentDto> locationAppointments, int externalId)
    {
        var scanTime = DateTime.UtcNow;
        var locationInformation =
            await appointmentLocationRepository.GetAppointmentLocationByExternalId(externalId);
        if (locationInformation == null) throw new ApplicationException("No location found");

        // Record first-seen times before any per-user filtering. The release delay is measured from
        // when a slot became visible to us, so it must be recorded even on scans where nobody is due.
        var firstSeenByStart = await seenAppointmentRepository.RecordAndGetFirstSeen(
            locationInformation.Id,
            locationAppointments.Select(x => x.StartTimestamp),
            scanTime);

        var trackersForLocation =
            await trackedLocationForUserRepository.GetTrackersByLocationIdDueForNotification(
                locationInformation.Id);
        if (trackersForLocation.Count == 0) return;

        var roles = roleManager.Roles.ToList();
        var userProfiles = await userProfileRepository.GetUserProfileByIds(
            trackersForLocation.Select(x => x.UserId).Distinct().ToList(), true);
        var profilesByUserId = userProfiles.ToDictionary(x => x.UserId);

        var notifiedProfiles = new List<UserProfileEntity>();
        foreach (var trackedLocationForUser in trackersForLocation)
        {
            if (!profilesByUserId.TryGetValue(trackedLocationForUser.UserId, out var userProfile))
            {
                logger.LogWarning("No profile found for user {UserId}, skipping notification",
                    trackedLocationForUser.UserId);
                continue;
            }

            var role = userRoleService.FindRoleForUser(userProfile, roles);
            if (role == null)
            {
                // One user with bad role data should not take down the whole location's batch.
                logger.LogWarning("No known role for user {UserId}, skipping notification",
                    trackedLocationForUser.UserId);
                continue;
            }

            var releasedAppointments = userAppointmentValidationService.GetAppointmentsReleasedToRole(
                locationAppointments, firstSeenByStart, role.NotificationDelayInMinutes, scanTime);
            if (releasedAppointments.Count == 0) continue;

            var validAppointments =
                userAppointmentValidationService.GetValidAppointmentsForUsers(releasedAppointments,
                    trackedLocationForUser);
            if (validAppointments.Count == 0) continue;

            await notificationManagerService.SendAppointmentAvailableNotifications(
                validAppointments,
                locationInformation, trackedLocationForUser.UserId,
                BuildDeliveryContext(role));

            // Only users who actually received a notification start a new cooldown. Users who were
            // due but had nothing released to them stay due, so they are not skipped for a cycle.
            userRoleService.SetNextNotificationTimeForUser(userProfile, role);
            if (!notifiedProfiles.Contains(userProfile)) notifiedProfiles.Add(userProfile);

            trackedLocationForUser.LastSeenEarliestAppointment =
                validAppointments.FirstOrDefault()?.StartTimestamp.ToUniversalTime();
        }

        await userProfileRepository.UpdateNextNotificationTimes(notifiedProfiles);
        await trackedLocationForUserRepository.UpdateTrackers(trackersForLocation);
    }

    /// <summary>
    ///     Builds the per-delivery context so a delayed alert can say so and link to the upgrade.
    /// </summary>
    private NotificationDeliveryContext BuildDeliveryContext(RoleEntity role)
    {
        if (role.NotificationDelayInMinutes <= 0) return NotificationDeliveryContext.Immediate;

        var baseUrl = configuration.GetValue<string>("Frontend:Base_Url");
        var upgradeEndpoint =
            configuration.GetValue("Frontend:Upgrade_Endpoint", "/settings/subscription");

        return new NotificationDeliveryContext
        {
            DelayInMinutes = role.NotificationDelayInMinutes,
            UpgradeUrl = string.IsNullOrWhiteSpace(baseUrl)
                ? null
                : $"{baseUrl.TrimEnd('/')}{upgradeEndpoint}"
        };
    }
}
