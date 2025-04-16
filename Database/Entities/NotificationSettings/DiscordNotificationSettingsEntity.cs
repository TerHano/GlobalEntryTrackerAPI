using System.ComponentModel.DataAnnotations;

namespace Database.Entities.NotificationSettings;

public class DiscordNotificationSettingsEntity : INotificationSettings
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public UserEntity User { get; set; }

    [MaxLength(2000)] public required string WebhookUrl { get; set; }
}