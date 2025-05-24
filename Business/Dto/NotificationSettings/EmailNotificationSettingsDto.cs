using System.ComponentModel.DataAnnotations;

namespace Business.Dto.NotificationSettings;

public class EmailNotificationSettingsDto
{
    [Required] public int Id { get; set; }

    [Required] public bool Enabled { get; set; }

    [Required] public string Email { get; set; } = string.Empty;
}