namespace Database.Entities;

public class TrackedLocationForUserEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required UserEntity User { get; set; }
    public int LocationId { get; set; }
    public required AppointmentLocationEntity Location { get; set; }
    public bool Enabled { get; set; }
    public int NotificationTypeId { get; set; }
    public NotificationTypeEntity NotificationType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}