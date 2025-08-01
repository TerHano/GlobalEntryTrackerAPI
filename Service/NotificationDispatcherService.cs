using Database.Entities;
using Database.Repositories;
using Microsoft.AspNetCore.Identity;
using Service.Dto;

namespace Service;

public class NotificationDispatcherService(
    NotificationManagerService notificationManagerService,
    UserAppointmentValidationService userAppointmentValidationService,
    RoleManager<RoleEntity> roleManager,
    UserRoleService userRoleService,
    TrackedLocationForUserRepository trackedLocationForUserRepository,
    UserProfileRepository userProfileRepository,
    AppointmentLocationRepository appointmentLocationRepository)
{
    //Create method that checks if a list of appointments is valid for each user tracking the location and then send a notification
    public async Task SendNotificationForLocation(
        List<LocationAppointmentDto> locationAppointments, int externalId)
    {
        var locationInformation =
            await appointmentLocationRepository.GetAppointmentLocationByExternalId(externalId);
        if (locationInformation == null) throw new ApplicationException("No location found");
        var trackersForLocation =
            await trackedLocationForUserRepository.GetTrackersByLocationIdDueForNotification(
                locationInformation.Id);

        foreach (var trackedLocationForUser in trackersForLocation)
        {
            var validAppointments =
                userAppointmentValidationService.GetValidAppointmentsForUsers(locationAppointments,
                    trackedLocationForUser);
            if (validAppointments.Count == 0) continue;
            await notificationManagerService.SendAppointmentAvailableNotifications(
                validAppointments,
                locationInformation, trackedLocationForUser.UserId);
            trackedLocationForUser.LastSeenEarliestAppointment =
                validAppointments.FirstOrDefault()?.StartTimestamp.ToUniversalTime();
        }

        await UpdateNextNotificationTimeBasedOnRole(trackersForLocation);
        await UpdateLastSeenEarliestAppointment(trackersForLocation);
    }

    private async Task UpdateNextNotificationTimeBasedOnRole(
        List<TrackedLocationForUserEntity> trackers)
    {
        var roles = roleManager.Roles.ToList();
        var userProfiles =
            await userProfileRepository.GetUserProfileByIds(trackers.Select(x => x.UserId)
                .Distinct()
                .ToList());
        foreach (var user in userProfiles)
            userRoleService.UpdateNextNotificationTimeForUser(user, roles);
        await userProfileRepository.UpdateMultipleUserProfiles(userProfiles);
    }

    //Update LastSeen Earliest Appointment for each user
    private async Task UpdateLastSeenEarliestAppointment(
        List<TrackedLocationForUserEntity> trackers)
    {
        await trackedLocationForUserRepository.UpdateTrackers(trackers);
    }
}