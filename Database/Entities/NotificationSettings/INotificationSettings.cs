namespace Database.Entities.NotificationSettings;

public class INotificationSettings
{
    public int Id { get; set; }
    public required bool Enabled { get; set; }
    public int? MaxNotificationsPerDay { get; set; }
    public int DailyNotificationCount { get; set; } = 0;
    public DateTime? DailyNotificationWindowStart { get; set; }
}