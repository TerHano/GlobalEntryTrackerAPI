using AutoMapper;
using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using Database.Entities.NotificationSettings;

namespace Business.Mappers;

public class DiscordNotificationSettingsMapper : Profile
{
    public DiscordNotificationSettingsMapper()
    {
        CreateMap<DiscordNotificationSettingsEntity, DiscordNotificationSettingsDto>().ReverseMap()
            .ForMember(dest => dest.DailyNotificationCount, opt => opt.Ignore())
            .ForMember(dest => dest.DailyNotificationWindowStart, opt => opt.Ignore());
        CreateMap<CreateDiscordSettingsRequest, DiscordNotificationSettingsEntity>()
            .ForMember(dest => dest.DailyNotificationCount, opt => opt.Ignore())
            .ForMember(dest => dest.DailyNotificationWindowStart, opt => opt.Ignore());
        CreateMap<UpdateDiscordSettingsRequest, DiscordNotificationSettingsEntity>()
            .ForMember(dest => dest.DailyNotificationCount, opt => opt.Ignore())
            .ForMember(dest => dest.DailyNotificationWindowStart, opt => opt.Ignore());
    }
}