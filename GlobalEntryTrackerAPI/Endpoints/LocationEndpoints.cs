using Business;
using Business.Dto;
using GlobalEntryTrackerAPI.Models;

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
                })
            .RequireAuthorization()
            .WithTags("Location")
            .WithName("GetAllLocations")
            .WithSummary("Get all appointment locations")
            .WithDescription(
                "Retrieves a list of all available appointment locations for the authenticated user.")
            .Produces<ApiResponse<AppointmentLocationDto[]>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapPost("/api/v1/location",
                async (AppointmentLocationBusiness appointmentLocationBusiness,
                    AppointmentLocationDto appointmentLocationDto) =>
                {
                    await appointmentLocationBusiness.CreateAppointmentLocation(
                        appointmentLocationDto);
                    return Results.Created($"/locations/{appointmentLocationDto.Id}",
                        appointmentLocationDto);
                })
            .RequireAuthorization()
            .WithTags("Location")
            .WithName("CreateLocation")
            .WithSummary("Create a new appointment location")
            .WithDescription("Creates a new appointment location for the authenticated user.")
            .Accepts<AppointmentLocationDto>("application/json")
            .Produces<ApiResponse<AppointmentLocationDto>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapGet("/api/v1/location/states",
                async (AppointmentLocationBusiness appointmentLocationBusiness) =>
                {
                    var states = await appointmentLocationBusiness.GetAllAppointmentStates();
                    return Results.Ok(states);
                })
            .RequireAuthorization()
            .WithTags("Location")
            .WithName("GetAllLocationStates")
            .WithSummary("Get all appointment location states")
            .WithDescription(
                "Retrieves a list of all available appointment location states for the authenticated user.")
            .Produces<ApiResponse<string[]>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}