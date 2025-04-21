namespace Business.Dto.Requests;

public class UpdateDiscordSettingsRequest
{
    public int Id { get; set; }
    public bool Enabled { get; set; }
    public string WebhookUrl { get; set; }
}