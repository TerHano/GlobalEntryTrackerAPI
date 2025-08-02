namespace Database.Entities.NotificationSettings;

public class EmailNotificationSettingsEntity : INotificationSettings
{
    public required string Email { get; set; } = string.Empty;

    public required UserNotificationEntity UserNotification { get; set; }
}