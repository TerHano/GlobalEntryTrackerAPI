using Database.Repositories;
using Microsoft.Extensions.Logging;
using Service.Dto;
using Service.Enum;
using Service.Notification;

namespace Service;

public class NotificationManagerService(
    ILogger<NotificationManagerService> logger,
    AppointmentLocationRepository appointmentLocationRepository,
    IServiceProvider serviceProvider)
{
    public async Task SendAppointmentAvailableNotifications(
        LocationAppointmentDto locationAppointment, int userId)
    {
        var locationInformation =
            await appointmentLocationRepository.GetAppointmentLocationByExternalId(
                locationAppointment.ExternalLocationId);
        if (locationInformation == null) throw new ApplicationException("No location found");
        var locationAppointmentWithDetails =
            new LocationAppointmentWithDetailsDto(locationAppointment, locationInformation);

        var notificationTasks = new List<Task>
        {
            SendNotificationForService(
                NotificationServiceType.Discord, locationAppointmentWithDetails, userId)
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
        LocationAppointmentWithDetailsDto locationAppointment, int userId)
    {
        var service = GetNotificationInstanceForService(serviceType);
        if (service == null)
        {
            logger.LogError("Service not found");
            return;
        }

        await service.SendNotification(locationAppointment, userId);
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
            default:
                throw new ApplicationException("Unknown notification service type");
        }

        return (INotificationService?)serviceProvider.GetService(
            serviceType);
    }
}