using System.ComponentModel.DataAnnotations;

namespace Business.Dto;

public class AppointmentLocationDto
{
    [Required] public int Id { get; init; }

    [Required] [MaxLength(100)] public required string Name { get; set; }

    [Required] [MaxLength(100)] public required string Address { get; set; }

    [MaxLength(100)] public string? AddressAdditional { get; set; }

    [Required] [MaxLength(100)] public required string City { get; set; }

    [Required] [MaxLength(100)] public required string State { get; set; }

    [Required] [MaxLength(15)] public required string PostalCode { get; set; }
    [Required] [MaxLength(100)] public required string Timezone { get; set; }
}