namespace Business.Dto.NotificationSettings;

public class DiscordNotificationSettingsDto
{
    public int Id { get; set; }
    public bool Enabled { get; set; }
    public string WebhookUrl { get; set; }
}