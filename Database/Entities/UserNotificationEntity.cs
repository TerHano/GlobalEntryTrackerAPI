using Database.Entities.NotificationSettings;

namespace Database.Entities;

public class UserNotificationEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public UserEntity User { get; set; }
    public int? DiscordNotificationSettingsId { get; set; }
    public DiscordNotificationSettingsEntity? DiscordNotificationSettings { get; set; }
    public int? EmailNotificationSettingsId { get; set; }
    public EmailNotificationSettingsEntity? EmailNotificationSettings { get; set; }
}