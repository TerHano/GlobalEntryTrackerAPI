using Database.Entities;
using Database.Repositories;
using Service.Dto;

namespace Service;

public class NotificationDispatcherService(
    NotificationManagerService notificationManagerService,
    UserAppointmentValidationService userAppointmentValidationService,
    UserRoleService userRoleService,
    TrackedLocationForUserRepository trackedLocationForUserRepository,
    UserRepository userRepository,
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
            trackedLocationForUserRepository.GetTrackersByLocationIdDueForNotification(
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
        var users = trackers.Select(x => x.User).Distinct().ToList();
        foreach (var user in users) userRoleService.UpdateNextNotificationTimeForUser(user);

        await userRepository.UpdateMultipleUsers(users);
    }

    //Update LastSeen Earliest Appointment for each user
    private async Task UpdateLastSeenEarliestAppointment(
        List<TrackedLocationForUserEntity> trackers)
    {
        await trackedLocationForUserRepository.UpdateTrackers(trackers);
    }
}