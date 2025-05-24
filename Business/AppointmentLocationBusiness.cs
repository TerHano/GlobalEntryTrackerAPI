using AutoMapper;
using Business.Dto;
using Database.Entities;
using Database.Repositories;

namespace Business;

/// <summary>
///     Handles business logic related to appointment locations.
/// </summary>
public class AppointmentLocationBusiness(
    AppointmentLocationRepository appointmentLocationRepository,
    IMapper mapper)
{
    /// <summary>
    ///     Retrieves all appointment locations, sorted by city name.
    /// </summary>
    /// <returns>List of appointment location DTOs.</returns>
    public async Task<List<AppointmentLocationDto>> GetAllAppointmentLocations()
    {
        var locations = await appointmentLocationRepository.GetAllAppointmentLocations();
        //Sort by city name
        locations.Sort((x, y) => string.Compare(x.City, y.City, StringComparison.Ordinal));
        return mapper.Map<List<AppointmentLocationDto>>(locations);
    }

    /// <summary>
    ///     Retrieves all appointment states, sorted alphabetically.
    /// </summary>
    /// <returns>List of state names.</returns>
    public async Task<List<string>> GetAllAppointmentStates()
    {
        var states = await appointmentLocationRepository.GetAppointmentStates();
        states.Sort();
        return mapper.Map<List<string>>(states);
    }

    /// <summary>
    ///     Retrieves an appointment location entity by its external ID.
    /// </summary>
    /// <param name="id">External ID of the appointment location.</param>
    /// <returns>The appointment location entity.</returns>
    public async Task<AppointmentLocationEntity> GetAppointmentLocationByExternalId(int id)
    {
        return await appointmentLocationRepository.GetAppointmentLocationByExternalId(id);
    }

    /// <summary>
    ///     Creates a new appointment location.
    /// </summary>
    /// <param name="appointmentLocation">The appointment location DTO.</param>
    public async Task CreateAppointmentLocation(AppointmentLocationDto appointmentLocation)
    {
        var appointmentLocationEntity = mapper.Map<AppointmentLocationEntity>(appointmentLocation);
        await appointmentLocationRepository.CreateAppointmentLocation(appointmentLocationEntity);
    }
}