namespace Business.Dto.Requests;

public class CreateTrackerForUserRequest
{
    public int LocationId { get; set; }
    public bool Enabled { get; set; }
    public int NotificationTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}