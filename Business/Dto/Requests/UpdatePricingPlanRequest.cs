using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class UpdatePricingPlanRequest
{
    [Required] public int Id { get; set; }

    [Required] public string Name { get; set; }

    [Required] public string Description { get; set; }

    [Required] public string PriceId { get; set; }

    [Required] public string Features { get; set; }
}