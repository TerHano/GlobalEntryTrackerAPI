using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class CreatePricingPlanRequest
{
    [Required] public string Name { get; set; } = null!;

    [Required] public string Description { get; set; } = null!;

    [Required] public string PriceId { get; set; } = null!;

    [Required] public string Features { get; set; } = null!;
}