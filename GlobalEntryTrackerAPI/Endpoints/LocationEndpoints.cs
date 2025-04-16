using Business;
using Business.Dto;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class LocationEndpoints
{
    public static void MapLocationEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/location",
            async (AppointmentLocationBusiness appointmentLocationBusiness) =>
            {
                var locations = await appointmentLocationBusiness.GetAllAppointmentLocations();
                return Results.Ok(locations);
            }).RequireAuthorization();

        app.MapPost("/api/v1/location",
            async (AppointmentLocationBusiness appointmentLocationBusiness,
                AppointmentLocationDto appointmentLocationDto) =>
            {
                await appointmentLocationBusiness.CreateAppointmentLocation(appointmentLocationDto);
                return Results.Created($"/locations/{appointmentLocationDto.Id}",
                    appointmentLocationDto);
            }).RequireAuthorization();
    }
}