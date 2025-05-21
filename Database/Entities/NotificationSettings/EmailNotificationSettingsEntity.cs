namespace Database.Entities.NotificationSettings;

public class EmailNotificationSettingsEntity : INotificationSettings
{
    public required string Email { get; set; } = string.Empty;

    public required int UserNotificationId { get; set; }
    public UserNotificationEntity UserNotification { get; set; }
}