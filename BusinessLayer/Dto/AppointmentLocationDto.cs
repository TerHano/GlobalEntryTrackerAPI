using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Dto;

public class AppointmentLocationDto
{
    public int Id { get; init; }
    [MaxLength(100)]
    public required string Name { get; set; }
    [MaxLength(100)]
    public required string Address { get; set; }
    [MaxLength(100)]
    public string? AddressAdditional { get; set; }
    [MaxLength(100)]
    public required string City { get; set; }
    [MaxLength(100)]
    public required string State { get; set; }
    [MaxLength(15)]
    public required string PostalCode { get; set; }
}