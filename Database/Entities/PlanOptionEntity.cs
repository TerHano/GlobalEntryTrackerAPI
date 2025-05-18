using System.ComponentModel.DataAnnotations;
using Database.Enums;

namespace Database.Entities;

public class PlanOptionEntity
{
    public int Id { get; set; }

    [MaxLength(100)] public required string Name { get; set; } = null!;

    [MaxLength(300)] public required string Description { get; set; } = null!;

    public required string PriceId { get; set; } = null!;
    public required int Price { get; set; }
    public int? DiscountedPrice { get; set; }
    public required string[] Features { get; set; } = null!;
    public required PlanOptionFrequency Frequency { get; set; }
}