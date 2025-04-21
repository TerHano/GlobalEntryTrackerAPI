namespace Business.Dto.Requests;

public class CreateDiscordSettingsRequest
{
    public bool Enabled { get; set; }
    public string WebhookUrl { get; set; }
}