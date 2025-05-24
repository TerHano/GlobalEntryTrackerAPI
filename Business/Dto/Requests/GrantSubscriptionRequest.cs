using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class GrantSubscriptionRequest
{
    [Required] public int UserId { get; set; }
    [Required] public string PriceId { get; set; }
}