namespace Database.Entities;

public class ArchivedAppointmentEntity
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public AppointmentLocationEntity Location { get; set; }
    public DateTime StartTimestamp { get; set; }
    public DateTime EndTimestamp { get; set; }
    public DateTime ScannedAt { get; set; }
}