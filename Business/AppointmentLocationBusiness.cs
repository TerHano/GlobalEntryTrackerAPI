using AutoMapper;
using Business.Dto;
using Database.Entities;
using Database.Repositories;

namespace Business;

public class AppointmentLocationBusiness(
    AppointmentLocationRepository appointmentLocationRepository,
    IMapper mapper)
{
    public async Task<List<AppointmentLocationDto>> GetAllAppointmentLocations()
    {
        var locations = await appointmentLocationRepository.GetAllAppointmentLocations();
        return mapper.Map<List<AppointmentLocationDto>>(locations);
    }

    public async Task<AppointmentLocationEntity> GetAppointmentLocationByExternalId(int id)
    {
        return await appointmentLocationRepository.GetAppointmentLocationByExternalId(id);
    }

    public async Task CreateAppointmentLocation(AppointmentLocationDto appointmentLocation)
    {
        var appointmentLocationEntity = mapper.Map<AppointmentLocationEntity>(appointmentLocation);
        await appointmentLocationRepository.CreateAppointmentLocation(appointmentLocationEntity);
    }
}