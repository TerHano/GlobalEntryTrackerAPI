using System.Globalization;
using AutoMapper;
using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;
using Database.Enums;
using Database.Repositories;
using Stripe;

namespace Business;

/// <summary>
///     Handles business logic for plan options.
/// </summary>
public class PlanBusiness(PlanOptionRepository planOptionRepository, IMapper mapper)
{
    /// <summary>
    ///     Retrieves all plan options, including Stripe pricing information.
    /// </summary>
    /// <returns>List of plan option DTOs.</returns>
    public async Task<List<PlanOptionDto>> GetPlanOptions()
    {
        var planOptions = await planOptionRepository.GetAllPlanOptions();
        var planOptionsDto = mapper.Map<List<PlanOptionDto>>(planOptions);
        var priceService = new PriceService();
        foreach (var planOptionDto in planOptionsDto)
        {
            var priceId = planOptionDto.PriceId;
            var price = await priceService.GetAsync(priceId);
            planOptionDto.Price = price.UnitAmount.GetValueOrDefault();
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

            planOptionDto.PriceFormatted = formatPrice(planOptionDto.Price, planOptionDto.Currency);
        }

        planOptionsDto = planOptionsDto.OrderBy(x => x.Price).ToList();
        return planOptionsDto;
    }

    public async Task<int> AddPlanOption(CreatePricingPlanRequest request)
    {
        var planOption = mapper.Map<PlanOptionEntity>(request);
        var planOptionId = await planOptionRepository.AddPlanOption(planOption);
        return planOptionId;
    }

    //update plan option
    public async Task UpdatePlanOption(UpdatePricingPlanRequest request)
    {
        var planOption = await planOptionRepository.GetPlanOptionById(request.Id);
        if (planOption == null)
            throw new Exception("Plan option not found");

        mapper.Map(request, planOption);
        await planOptionRepository.UpdatePlanOption(planOption);
    }

    public async Task DeletePlanOption(int id)
    {
        var planOption = await planOptionRepository.GetPlanOptionById(id);
        if (planOption == null)
            throw new Exception("Plan option not found");

        await planOptionRepository.DeletePlanOption(id);
    }

    private string formatPrice(long price, string currency)
    {
        var cultureInfo = new CultureInfo("en-US");
        return string.Format(cultureInfo, "{0:C}", price / 100.0);
    }
}