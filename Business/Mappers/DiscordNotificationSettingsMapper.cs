using AutoMapper;
using Business.Dto.NotificationSettings;
using Database.Entities.NotificationSettings;

namespace Business.Mappers;

public class DiscordNotificationSettingsMapper : Profile
{
    public DiscordNotificationSettingsMapper()
    {
        CreateMap<DiscordNotificationSettingsEntity, DiscordNotificationSettingsDto>().ReverseMap();
    }
}