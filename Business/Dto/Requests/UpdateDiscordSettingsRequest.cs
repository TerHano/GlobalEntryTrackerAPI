using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class UpdateDiscordSettingsRequest
{
    [Required] public int Id { get; set; }

    [Required] public bool Enabled { get; set; }

    [Required] public string WebhookUrl { get; set; }
}