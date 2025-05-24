using System.ComponentModel.DataAnnotations;

namespace Business.Dto;

public class TrackedLocationForUserDto
{
    [Required] public int Id { get; set; }

    [Required] public AppointmentLocationDto Location { get; set; }

    [Required] public bool Enabled { get; set; }

    [Required] public NotificationTypeDto NotificationType { get; set; }

    [Required] public DateOnly CutOffDate { get; set; }

    public DateTime? LastSeenEarliestAppointment { get; set; }
}