using AutoMapper;
using BusinessLayer.Dto;
using Database.Entities;
using Database.Repositories;

namespace BusinessLayer;

public class AppointmentLocationBusiness(AppointmentLocationRepository appointmentLocationRepository, IMapper mapper)
{
    public List<AppointmentLocationEntity> GetAllAppointmentLocations()
    {
        return appointmentLocationRepository.GetAllAppointmentLocations();
    }

    public AppointmentLocationEntity GetAppointmentLocationByExternalId(int id)
    {
        return appointmentLocationRepository.GetAppointmentLocationByExternalId(id);
    }

    public void AddAppointmentLocation(AppointmentLocationDto appointmentLocation)
    {
        var appointmentLocationEntity = mapper.Map<AppointmentLocationEntity>(appointmentLocation);
        appointmentLocationRepository.AddAppointmentLocation(appointmentLocationEntity);
    }
}