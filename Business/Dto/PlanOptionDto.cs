using Database.Enums;

namespace Business.Dto;

public class PlanOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string PriceId { get; set; } = null!;
    public long Price { get; set; }
    public string Currency { get; set; } = null!;
    public List<string> Features { get; set; } = null!;
    public PlanOptionFrequency Frequency { get; set; }
}