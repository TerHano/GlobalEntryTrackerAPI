using AutoMapper;
using Business.Dto;
using Database.Enums;
using Database.Repositories;
using Stripe;

namespace Business;

public class PlanBusiness(PlanOptionRepository planOptionRepository, IMapper mapper)
{
    public async Task<List<PlanOptionDto>> GetPlanOptions()
    {
        var planOptions = await planOptionRepository.GetAllPlanOptions();
        var planOptionsDto = mapper.Map<List<PlanOptionDto>>(planOptions);
        var priceService = new PriceService();
        foreach (var planOptionDto in planOptionsDto)
        {
            var priceId = planOptionDto.PriceId;
            var price = await priceService.GetAsync(priceId);
            planOptionDto.Price = price.UnitAmount.Value;
            planOptionDto.Currency = price.Currency;
            switch (price.Recurring.Interval)
            {
                case "month":
                    planOptionDto.Frequency = PlanOptionFrequency.Monthly;
                    break;
                case "week":
                    planOptionDto.Frequency = PlanOptionFrequency.Weekly;
                    break;
                default:
                    planOptionDto.Frequency = PlanOptionFrequency.Monthly;
                    break;
            }
        }

        planOptionsDto = planOptionsDto.OrderBy(x => x.Price).ToList();
        return planOptionsDto;
    }
}