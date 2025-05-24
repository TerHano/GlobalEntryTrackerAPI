using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class TestDiscordSettingsRequest
{
    [Required] public string WebhookUrl { get; set; }
}