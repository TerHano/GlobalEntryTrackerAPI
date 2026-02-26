using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class CreateEmailNotificationSettingsRequest
{
    [Required] public bool Enabled { get; set; }

    public int? MaxNotificationsPerDay { get; set; }
}