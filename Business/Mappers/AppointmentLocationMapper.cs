using AutoMapper;
using Business.Dto;
using Database.Entities;

namespace Business.Mappers;

public class AppointmentLocationMapper : Profile
{
    public AppointmentLocationMapper()
    {
        CreateMap<AppointmentLocationEntity, AppointmentLocationDto>().ReverseMap();
    }
}