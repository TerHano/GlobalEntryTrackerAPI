namespace Database.Entities.NotificationSettings;

public class INotificationSettings
{
    public int Id { get; set; }
    public bool Enabled { get; set; }
    public int UserNotificationId { get; set; }
    public UserNotificationEntity UserNotification { get; set; }
}