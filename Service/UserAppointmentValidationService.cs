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
        const int maxAppointmentsForDayToNotify = 3;
        const int maxDaysToNotify = 5;
        var validLocationAppointments = new List<LocationAppointmentDto>();
        var notificationType = trackedLocationForUser.NotificationType.Type;
        switch (notificationType)
        {
            case NotificationType.Soonest:
            {
                DateTime? lastSeenDate = null;
                var locationsSeenForDate = 0;
                var daysSeen = 0;
                foreach (var locationAppointmentDto in locationAppointments)
                {
                    if (lastSeenDate == locationAppointmentDto.StartTimestamp.Date &&
                        locationsSeenForDate >= maxAppointmentsForDayToNotify)
                        continue;
                    if (lastSeenDate != locationAppointmentDto.StartTimestamp.Date)
                    {
                        // Reset the counter for a new date
                        locationsSeenForDate = 0;
                        daysSeen++;
                        if (daysSeen > maxDaysToNotify)
                            break;
                    }

                    lastSeenDate = locationAppointmentDto.StartTimestamp.Date;
                    var locationAppointmentDate =
                        DateOnly.FromDateTime(locationAppointmentDto.StartTimestamp);
                    if (locationAppointmentDate <= trackedLocationForUser.CutOffDate)
                    {
                        validLocationAppointments.Add(locationAppointmentDto);
                        locationsSeenForDate++;
                    }
                }

                break;
            }
            case NotificationType.Weekends:
            {
                foreach (var locationAppointmentDto in locationAppointments)
                {
                    if (locationAppointmentDto.StartTimestamp.DayOfWeek is DayOfWeek.Saturday
                        or DayOfWeek.Sunday)
                        validLocationAppointments.Add(locationAppointmentDto);

                    if (validLocationAppointments.Count >= maxAppointmentsForDayToNotify) break;
                }
            }

                break;
        }

        return validLocationAppointments;
    }
}