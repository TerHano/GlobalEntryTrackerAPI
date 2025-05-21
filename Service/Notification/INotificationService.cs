using Database.Entities;
using Service.Dto;

namespace Service.Notification;

public interface INotificationService
{
    Task SendNotification<T>(List<LocationAppointmentDto> appointments,
        AppointmentLocationEntity locationInformation, T userNotificationSettings);

    Task SendTestNotification<T>(T settingsToTest);
}