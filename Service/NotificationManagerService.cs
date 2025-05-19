using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.Logging;
using Service.Dto;
using Service.Enum;
using Service.Notification;

namespace Service;

public class NotificationManagerService(
    ILogger<NotificationManagerService> logger,
    UserNotificationRepository userNotificationRepository,
    IServiceProvider serviceProvider)
{
    public async Task SendAppointmentAvailableNotifications(
        List<LocationAppointmentDto> locationAppointments,
        AppointmentLocationEntity appointmentLocation, int userId)
    {
        var userNotification =
            await userNotificationRepository.GetUserWithNotificationSettings(userId);
        var notificationTasks = new List<Task>
        {
            SendNotificationForService(
                NotificationServiceType.Discord, locationAppointments,
                appointmentLocation, userNotification.DiscordNotificationSettingsId),
            SendNotificationForService(NotificationServiceType.Email,
                locationAppointments, appointmentLocation,
                userNotification.EmailNotificationSettingsId)
        };
        await Task.WhenAll(notificationTasks);
    }

    public async Task SendTestMessageForService<T>(NotificationServiceType notificationServiceType,
        T notificationSettings)
    {
        var service = GetNotificationInstanceForService(notificationServiceType);
        if (service == null) throw new ApplicationException("No service found");
        await service.SendTestNotification(notificationSettings);
    }

    private async Task SendNotificationForService(NotificationServiceType serviceType,
        List<LocationAppointmentDto> locationAppointments,
        AppointmentLocationEntity locationInformation, int? userNotificationId)
    {
        var service = GetNotificationInstanceForService(serviceType);
        if (service == null)
        {
            logger.LogError("Service not found");
            return;
        }

        await service.SendNotification(locationAppointments, locationInformation,
            userNotificationId);
    }

    private INotificationService? GetNotificationInstanceForService(
        NotificationServiceType notificationServiceType)
    {
        Type serviceType;
        switch (notificationServiceType)
        {
            case NotificationServiceType.Discord:
                serviceType = typeof(DiscordNotificationService);
                break;
            case NotificationServiceType.Email:
                serviceType = typeof(EmailNotificationService);
                break;
            default:
                throw new ApplicationException("Unknown notification service type");
        }

        return (INotificationService?)serviceProvider.GetService(
            serviceType);
    }
}