namespace Database.Entities.NotificationSettings;

public class INotificationSettings
{
    public int Id { get; set; }
    public required bool Enabled { get; set; }
}