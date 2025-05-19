namespace Business.Dto.Requests;

public class UpdateEmailNotificationSettingsRequest
{
    public int Id { get; set; }
    public bool Enabled { get; set; }
}