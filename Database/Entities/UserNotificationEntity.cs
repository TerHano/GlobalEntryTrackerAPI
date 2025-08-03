using Database.Entities.NotificationSettings;

namespace Database.Entities;

public class UserNotificationEntity
{
    public int Id { get; set; }
    public string UserId { get; init; } = null!;
    public UserEntity User { get; init; } = null!;
    public int? DiscordNotificationSettingsId { get; set; }
    public DiscordNotificationSettingsEntity? DiscordNotificationSettings { get; set; }
    public int? EmailNotificationSettingsId { get; set; }
    public EmailNotificationSettingsEntity? EmailNotificationSettings { get; set; }
}