using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class UpdateTrackerForUserRequest
{
    [Required] public int Id { get; set; }

    [Required] public int LocationId { get; set; }

    [Required] public bool Enabled { get; set; }

    [Required] public int NotificationTypeId { get; set; }

    [Required] public DateOnly CutOffDate { get; set; }
}