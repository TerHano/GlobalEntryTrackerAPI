using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class UpdateEmailNotificationSettingsRequest
{
    [Required] public int Id { get; set; }

    [Required] public bool Enabled { get; set; }
}