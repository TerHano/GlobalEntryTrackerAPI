namespace Business.Dto.Requests;

public class CreateTrackerForUserRequest
{
    public int LocationId { get; set; }
    public bool Enabled { get; set; }
    public int NotificationTypeId { get; set; }
    public DateOnly CutOffDate { get; set; }
}