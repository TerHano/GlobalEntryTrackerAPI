using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class CreateDiscordSettingsRequest
{
    [Required] public required bool Enabled { get; set; }

    [Required] public string WebhookUrl { get; set; }
}