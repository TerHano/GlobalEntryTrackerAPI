using AutoMapper;
using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using Database.Entities.NotificationSettings;

namespace Business.Mappers;

public class DiscordNotificationSettingsMapper : Profile
{
    public DiscordNotificationSettingsMapper()
    {
        CreateMap<DiscordNotificationSettingsEntity, DiscordNotificationSettingsDto>().ReverseMap();
        CreateMap<CreateDiscordSettingsRequest, DiscordNotificationSettingsEntity>();
        CreateMap<UpdateDiscordSettingsRequest, DiscordNotificationSettingsEntity>();
    }
}