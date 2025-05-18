using AutoMapper;
using Business.Dto;
using Database.Entities;

namespace Business.Mappers;

public class PlanOptionMapper : Profile
{
    public PlanOptionMapper()
    {
        CreateMap<PlanOptionEntity, PlanOptionDto>();
    }
}