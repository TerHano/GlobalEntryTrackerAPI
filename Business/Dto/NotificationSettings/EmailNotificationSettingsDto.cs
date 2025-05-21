namespace Business.Dto.NotificationSettings;

public class EmailNotificationSettingsDto
{
    public int Id { get; set; }
    public bool Enabled { get; set; }
    public string Email { get; set; } = string.Empty;
}