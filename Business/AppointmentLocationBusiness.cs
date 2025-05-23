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
        //Sort by city name
        locations.Sort((x, y) => string.Compare(x.City, y.City, StringComparison.Ordinal));
        return mapper.Map<List<AppointmentLocationDto>>(locations);
    }

    //Get all appointment states sorted

    public async Task<List<string>> GetAllAppointmentStates()
    {
        var states = await appointmentLocationRepository.GetAppointmentStates();
        states.Sort();
        return mapper.Map<List<string>>(states);
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