using System.Text.Json.Serialization;

namespace Service.Dto;

public class LocationAppointmentDto
{
    [JsonPropertyName("locationId")] public int ExternalLocationId { get; set; }
    public DateTime StartTimestamp { get; set; }
    public DateTime EndTimestamp { get; set; }
    public bool Active { get; set; }
    public int Duration { get; set; }
    public bool RemoteInd { get; set; }
}