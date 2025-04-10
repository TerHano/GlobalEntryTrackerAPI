using Service.Dto;

namespace Service.Notification;

public interface INotificationService
{
    Task SendNotification(LocationAppointmentWithDetailsDto appointment, int userId);

    Task SendTestNotification<T>(T settingsToTest);
}