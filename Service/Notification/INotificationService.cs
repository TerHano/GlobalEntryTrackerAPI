using Database.Entities;
using Service.Dto;

namespace Service.Notification;

public interface INotificationService
{
    Task SendNotification(List<LocationAppointmentDto> appointments,
        AppointmentLocationEntity locationInformation, int userId);

    Task SendTestNotification<T>(T settingsToTest);
}