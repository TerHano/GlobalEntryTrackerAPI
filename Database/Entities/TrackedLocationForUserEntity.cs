namespace Database.Entities;

public class TrackedLocationForUserEntity
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public UserEntity User { get; set; }
    public required int LocationId { get; set; }
    public AppointmentLocationEntity Location { get; set; }
    public bool Enabled { get; set; }
    public required int NotificationTypeId { get; set; }
    public NotificationTypeEntity NotificationType { get; set; }
    public required DateOnly CutOffDate { get; set; }

    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
}