namespace Business.Dto;

public class UserSubscriptionDto
{
    public bool Active { get; set; }

    public string PlanName { get; set; } = string.Empty;
    public long PlanPrice { get; set; }
    public string Currency { get; set; }
    public string PlanInterval { get; set; }

    public bool IsEnding { get; set; }
    public DateOnly NextPaymentDate { get; set; }
    public string CardBrand { get; set; } = string.Empty;
    public string CardLast4Digits { get; set; }
}