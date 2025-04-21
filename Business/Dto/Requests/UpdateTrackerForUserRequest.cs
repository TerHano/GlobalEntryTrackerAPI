namespace Business.Dto.Requests;

public class UpdateTrackerForUserRequest
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public bool Enabled { get; set; }
    public int NotificationTypeId { get; set; }
    public DateOnly CutOffDate { get; set; }
}