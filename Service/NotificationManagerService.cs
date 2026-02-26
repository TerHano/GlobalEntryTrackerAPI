using Database.Entities;
using Database.Entities.NotificationSettings;
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
        AppointmentLocationEntity appointmentLocation, string userId)
    {
        var userNotification =
            await userNotificationRepository.GetUserWithNotificationSettings(userId);

        var discordSettings = userNotification.DiscordNotificationSettings;
        var emailSettings = userNotification.EmailNotificationSettings;

        bool discordAllowed = IsWithinDailyLimit(discordSettings);
        bool emailAllowed = IsWithinDailyLimit(emailSettings);

        var notificationTasks = new List<Task>();

        if (discordAllowed)
            notificationTasks.Add(
                SendNotificationForService(
                    NotificationServiceType.Discord, locationAppointments,
                    appointmentLocation, discordSettings));

        if (emailAllowed)
            notificationTasks.Add(
                SendNotificationForService(NotificationServiceType.Email,
                    locationAppointments, appointmentLocation,
                    userNotification.EmailNotificationSettings));

        await Task.WhenAll(notificationTasks);

        if (discordAllowed && discordSettings != null)
            IncrementDailyCount(discordSettings);
        if (emailAllowed && emailSettings != null)
            IncrementDailyCount(emailSettings);

        if (discordAllowed || emailAllowed)
            await userNotificationRepository.UpdateChannelNotificationCounts(
                discordAllowed ? discordSettings : null,
                emailAllowed ? emailSettings : null);
    }

    public async Task SendTestMessageForService<T>(NotificationServiceType notificationServiceType,
        T notificationSettings)
    {
        var service = GetNotificationInstanceForService(notificationServiceType);
        if (service == null) throw new ApplicationException("No service found");
        await service.SendTestNotification(notificationSettings);
    }

    private static bool IsWithinDailyLimit(INotificationSettings? settings)
    {
        if (settings == null || !settings.Enabled) return false;
        if (!settings.MaxNotificationsPerDay.HasValue) return true;

        // Reset the rolling window if more than 24 hours have passed
        if (settings.DailyNotificationWindowStart.HasValue &&
            DateTime.UtcNow > settings.DailyNotificationWindowStart.Value.AddHours(24))
        {
            settings.DailyNotificationCount = 0;
            settings.DailyNotificationWindowStart = null;
        }

        return settings.DailyNotificationCount < settings.MaxNotificationsPerDay.Value;
    }

    private static void IncrementDailyCount(INotificationSettings settings)
    {
        if (settings.DailyNotificationWindowStart == null)
            settings.DailyNotificationWindowStart = DateTime.UtcNow;
        settings.DailyNotificationCount++;
    }

    private async Task SendNotificationForService<T>(NotificationServiceType serviceType,
        List<LocationAppointmentDto> locationAppointments,
        AppointmentLocationEntity locationInformation, T userNotificationSettings)
    {
        var service = GetNotificationInstanceForService(serviceType);
        if (service == null)
        {
            logger.LogError("Service not found");
            return;
        }

        await service.SendNotification(locationAppointments, locationInformation,
            userNotificationSettings);
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