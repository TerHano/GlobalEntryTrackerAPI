using System.ComponentModel.DataAnnotations;

namespace Database.Entities;

public class AppointmentLocationEntity
{
    public int Id { get; init; }
    public int ExternalId { get; init; }

    [MaxLength(100)] public required string Name { get; set; }

    [MaxLength(100)] public required string Address { get; set; }

    [MaxLength(100)] public string? AddressAdditional { get; set; }

    [MaxLength(100)] public required string City { get; set; }

    [MaxLength(100)] public required string State { get; set; }

    [MaxLength(15)] public required string PostalCode { get; set; }
    [MaxLength(100)] public string Timezone { get; set; }
}