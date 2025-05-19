namespace Business.Dto.NotificationSettings;

public class UserNotificationSettingsDto
{
    public DiscordNotificationSettingsDto? DiscordSettings { get; set; }
    public EmailNotificationSettingsDto? EmailSettings { get; set; }
}