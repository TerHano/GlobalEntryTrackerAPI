namespace Database.Entities;

public class ArchivedAppointmentEntity
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public AppointmentLocationEntity Location { get; set; }
    public DateOnly Date { get; set; }
    public int NumberOfAppointments { get; set; }
    public DateTime ScannedAt { get; set; }
}