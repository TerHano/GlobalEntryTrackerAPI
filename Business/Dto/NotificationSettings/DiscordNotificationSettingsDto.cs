namespace Business.Dto.NotificationSettings;

public class DiscordNotificationSettingsDto
{
    public bool Enabled { get; set; }
    public string WebhookUrl { get; set; }
}