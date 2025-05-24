using System.ComponentModel.DataAnnotations;
using Database.Enums;

namespace Business.Dto;

public class PlanOptionDto
{
    [Required] public int Id { get; set; }

    [Required] public string Name { get; set; } = null!;

    [Required] public string Description { get; set; } = null!;

    [Required] public string PriceId { get; set; } = null!;

    [Required] public long Price { get; set; }

    [Required] public string PriceFormatted { get; set; } = null!;

    [Required] public string Currency { get; set; } = null!;

    [Required] public List<string> Features { get; set; } = null!;

    [Required] public PlanOptionFrequency Frequency { get; set; }
}