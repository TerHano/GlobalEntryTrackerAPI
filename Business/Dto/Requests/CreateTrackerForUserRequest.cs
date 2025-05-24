using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class CreateTrackerForUserRequest
{
    [Required] public int LocationId { get; set; }

    [Required] public bool Enabled { get; set; }

    [Required] public int NotificationTypeId { get; set; }

    [Required] public DateOnly CutOffDate { get; set; }
}