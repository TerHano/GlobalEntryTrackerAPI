using GlobalEntryTrackerAPI;

namespace Business.Dto.Requests;

public class UpdateTrackerForUserRequest
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public bool Enabled { get; set; }
    public NotificationType NotificationType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}