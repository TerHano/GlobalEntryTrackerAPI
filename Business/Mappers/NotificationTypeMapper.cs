using AutoMapper;
using Business.Dto;
using Database.Entities;

namespace Business.Mappers;

public class NotificationTypeMapper : Profile
{
    public NotificationTypeMapper()
    {
        CreateMap<NotificationTypeEntity, NotificationTypeDto>().ReverseMap();
    }
}