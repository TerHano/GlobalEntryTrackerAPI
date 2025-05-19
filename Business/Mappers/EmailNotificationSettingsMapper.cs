using AutoMapper;
using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using Database.Entities.NotificationSettings;

namespace Business.Mappers;

public class EmailNotificationSettingsMapper : Profile
{
    public EmailNotificationSettingsMapper()
    {
        CreateMap<EmailNotificationSettingsEntity, EmailNotificationSettingsDto>().ReverseMap();
        CreateMap<CreateEmailNotificationSettingsRequest, EmailNotificationSettingsEntity>();
        CreateMap<UpdateEmailNotificationSettingsRequest, EmailNotificationSettingsEntity>();
    }
}