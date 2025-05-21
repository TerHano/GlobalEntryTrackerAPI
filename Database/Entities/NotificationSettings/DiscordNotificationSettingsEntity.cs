using System.ComponentModel.DataAnnotations;

namespace Database.Entities.NotificationSettings;

public class DiscordNotificationSettingsEntity : INotificationSettings
{
    [MaxLength(2000)] public required string WebhookUrl { get; set; }
    public required int UserNotificationId { get; set; }
    public UserNotificationEntity UserNotification { get; set; }
}