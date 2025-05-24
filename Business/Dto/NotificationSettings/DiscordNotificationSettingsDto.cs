using System.ComponentModel.DataAnnotations;

namespace Business.Dto.NotificationSettings;

public class DiscordNotificationSettingsDto
{
    public int Id { get; set; }

    [Required] public bool Enabled { get; set; }

    [Required] public string WebhookUrl { get; set; }
}