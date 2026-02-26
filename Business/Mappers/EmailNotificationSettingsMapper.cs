using AutoMapper;
using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using Database.Entities.NotificationSettings;

namespace Business.Mappers;

public class EmailNotificationSettingsMapper : Profile
{
    public EmailNotificationSettingsMapper()
    {
        CreateMap<EmailNotificationSettingsEntity, EmailNotificationSettingsDto>().ReverseMap()
            .ForMember(dest => dest.DailyNotificationCount, opt => opt.Ignore())
            .ForMember(dest => dest.DailyNotificationWindowStart, opt => opt.Ignore());
        CreateMap<CreateEmailNotificationSettingsRequest, EmailNotificationSettingsEntity>()
            .ForMember(dest => dest.DailyNotificationCount, opt => opt.Ignore())
            .ForMember(dest => dest.DailyNotificationWindowStart, opt => opt.Ignore());
        CreateMap<UpdateEmailNotificationSettingsRequest, EmailNotificationSettingsEntity>()
            .ForMember(dest => dest.DailyNotificationCount, opt => opt.Ignore())
            .ForMember(dest => dest.DailyNotificationWindowStart, opt => opt.Ignore());
    }
}