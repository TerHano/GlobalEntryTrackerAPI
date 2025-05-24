using AutoMapper;
using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;

namespace Business.Mappers;

public class PlanOptionMapper : Profile
{
    public PlanOptionMapper()
    {
        CreateMap<PlanOptionEntity, PlanOptionDto>();
        CreateMap<CreatePricingPlanRequest, PlanOptionEntity>()
            .ForMember(dest => dest.Features,
                opt => opt.MapFrom(src => SplitFeatures(src.Features)))
            .ForMember(dest => dest.PriceId, opt => opt.MapFrom(src => src.PriceId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

        CreateMap<UpdatePricingPlanRequest, PlanOptionEntity>()
            .ForMember(dest => dest.Features,
                opt => opt.MapFrom(src => SplitFeatures(src.Features)))
            .ForMember(dest => dest.PriceId, opt => opt.MapFrom(src => src.PriceId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
    }

    private static string[] SplitFeatures(string features)
    {
        // Split the features string by commas, trim and return as an array.
        var featureArr = features.Split(",");
        for (var i = 0; i < featureArr.Length; i++) featureArr[i] = featureArr[i].Trim();
        return featureArr;
    }
}