using System.ComponentModel.DataAnnotations;

namespace Business.Dto;

public class UserSubscriptionDto
{
    [Required] public bool ActiveBilledSubscription { get; set; }

    [Required] public string PlanName { get; set; } = string.Empty;

    [Required] public long PlanPrice { get; set; }

    public string Currency { get; set; }

    [Required] public string PlanInterval { get; set; }

    [Required] public bool IsEnding { get; set; }

    public DateOnly? NextPaymentDate { get; set; }

    public string? CardBrand { get; set; } = string.Empty;
    public string? CardLast4Digits { get; set; }
}