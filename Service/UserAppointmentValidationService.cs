using Database.Entities;
using GlobalEntryTrackerAPI;
using Service.Dto;

namespace Service;

public class UserAppointmentValidationService
{
    public List<LocationAppointmentDto> GetValidAppointmentsForUsers(
        List<LocationAppointmentDto> locationAppointments,
        TrackedLocationForUserEntity trackedLocationForUser)
    {
        const int maxAppointmentToNotify = 5;
        var validLocationAppointments = new List<LocationAppointmentDto>();
        var notificationType = trackedLocationForUser.NotificationType.Type;
        switch (notificationType)
        {
            case NotificationType.Before:
            {
                foreach (var locationAppointmentDto in locationAppointments)
                {
                    var locationAppointmentDate =
                        DateOnly.FromDateTime(locationAppointmentDto.StartTimestamp);
                    if (locationAppointmentDate <= trackedLocationForUser.StartDate)
                        validLocationAppointments.Add(locationAppointmentDto);
                    if (validLocationAppointments.Count >= maxAppointmentToNotify) break;
                }

                break;
            }
            case NotificationType.Between:
            {
                foreach (var locationAppointmentDto in locationAppointments)
                {
                    var locationAppointmentDate =
                        DateOnly.FromDateTime(locationAppointmentDto.StartTimestamp);
                    if (locationAppointmentDate >= trackedLocationForUser.StartDate &&
                        locationAppointmentDate <= trackedLocationForUser.EndDate)
                        validLocationAppointments.Add(locationAppointmentDto);
                    if (validLocationAppointments.Count >= maxAppointmentToNotify) break;
                }

                break;
            }
        }

        return validLocationAppointments;
    }
}