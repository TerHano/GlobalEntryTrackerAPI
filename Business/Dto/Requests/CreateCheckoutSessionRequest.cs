using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class CreateCheckoutSessionRequest
{
    [Required] public string PriceId { get; set; } = string.Empty;

    [Required] public string SuccessUrl { get; set; } = string.Empty;

    [Required] public string CancelUrl { get; set; } = string.Empty;
}