namespace Business.Dto;

public class TrackedLocationForUserDto
{
    public int Id { get; set; }
    public AppointmentLocationDto Location { get; set; }
    public bool Enabled { get; set; }
    public NotificationTypeDto NotificationType { get; set; }
    public DateOnly CutOffDate { get; set; }
    public DateTime? LastSeenEarliestAppointment { get; set; }
}