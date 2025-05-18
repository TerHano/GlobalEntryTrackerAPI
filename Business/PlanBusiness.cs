using AutoMapper;
using Business.Dto;
using Database.Repositories;

namespace Business;

public class PlanBusiness(PlanOptionRepository planOptionRepository, IMapper mapper)
{
    public async Task<List<PlanOptionDto>> GetPlanOptions()
    {
        var planOptions = await planOptionRepository.GetAllPlanOptions();
        var planOptionsDto = mapper.Map<List<PlanOptionDto>>(planOptions);
        return planOptionsDto;
    }
}