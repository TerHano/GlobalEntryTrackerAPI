using GlobalEntryTrackerAPI;

namespace Database.Entities;

public class TrackedLocationForUserEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LocationId { get; set; }
    public bool Enabled { get; set; }
    public NotificationType NotificationType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}