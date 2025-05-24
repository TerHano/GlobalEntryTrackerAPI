using System.ComponentModel.DataAnnotations;

namespace Business.Dto;

public class NotificationCheckDto
{
    [Required] public bool IsAnyNotificationsEnabled { get; set; }
}