using Database.Entities;

namespace Service.Dto;

public class LocationAppointmentWithDetailsDto(
    LocationAppointmentDto locationAppointmentDto,
    AppointmentLocationEntity appointmentLocationEntity)
{
    public int ExternalLocationId { get; set; } = locationAppointmentDto.ExternalLocationId;
    public AppointmentLocationEntity Location { get; set; } = appointmentLocationEntity;
    public DateTime StartTimestamp { get; set; } = locationAppointmentDto.StartTimestamp;
    public DateTime EndTimestamp { get; set; } = locationAppointmentDto.EndTimestamp;
    public bool Active { get; set; } = locationAppointmentDto.Active;
    public int Duration { get; set; } = locationAppointmentDto.Duration;
    public bool RemoteInd { get; set; } = locationAppointmentDto.RemoteInd;
}