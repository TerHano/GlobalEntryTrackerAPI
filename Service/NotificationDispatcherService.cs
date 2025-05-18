using Database.Entities;
using Database.Repositories;
using Service.Dto;

namespace Service;

public class NotificationDispatcherService(
    NotificationManagerService notificationManagerService,
    UserAppointmentValidationService userAppointmentValidationService,
    TrackedLocationForUserRepository trackedLocationForUserRepository,
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
        }

        await UpdateNextNotificationTimeBasedOnRole(trackersForLocation);
    }

    private async Task UpdateNextNotificationTimeBasedOnRole(
        List<TrackedLocationForUserEntity> trackers)
    {
        foreach (var trackedLocationForUser in trackers)
        {
            var userRoleIntervalInMinutes =
                trackedLocationForUser.User.UserRoles.Max(r =>
                    r.Role.NotificationIntervalInMinutes);
            trackedLocationForUser.NextNotificationAt =
                trackedLocationForUser.NextNotificationAt.AddMinutes(userRoleIntervalInMinutes);
        }

        await trackedLocationForUserRepository.UpdateListOfTrackers(trackers);
    }
}