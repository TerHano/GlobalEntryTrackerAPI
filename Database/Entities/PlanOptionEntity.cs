using System.ComponentModel.DataAnnotations;

namespace Database.Entities;

public class PlanOptionEntity
{
    public int Id { get; set; }

    [MaxLength(100)] public required string Name { get; set; } = null!;
    [MaxLength(300)] public required string Description { get; set; } = null!;
    public required string PriceId { get; set; } = null!;
    public required string[] Features { get; set; } = null!;
}