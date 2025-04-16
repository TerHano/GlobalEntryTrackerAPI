using Business;
using Business.Dto.Requests;
using GlobalEntryTrackerAPI.Extensions;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class LocationTrackerEndpoints
{
    public static void MapLocationTrackerEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/tracked-locations", async (HttpContext httpContext,
            UserAppointmentTrackerBusiness userAppointmentTrackerBusiness) =>
        {
            var userId = httpContext.User.GetUserId();
            var trackedLocations =
                await userAppointmentTrackerBusiness.GetTrackedAppointmentLocationsForUser(userId);
            return Results.Ok(trackedLocations);
        }).RequireAuthorization();

        app.MapGet("/api/v1/tracked-locations/{id}", async (int id, HttpContext httpContext,
            UserAppointmentTrackerBusiness userAppointmentTrackerBusiness) =>
        {
            var userId = httpContext.User.GetUserId();
            var trackedLocations =
                await userAppointmentTrackerBusiness.GetTrackedAppointmentLocationById(id, userId);
            return Results.Ok(trackedLocations);
        }).RequireAuthorization();

        app.MapPost("/api/v1/track-location",
            async (CreateTrackerForUserRequest request,
                HttpContext httpContext,
                UserAppointmentTrackerBusiness userAppointmentTrackerBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var createdId =
                    await userAppointmentTrackerBusiness.CreateTrackerForUser(request, userId);
                return Results.Created($"/api/v1/tracked-locations/{createdId}", createdId);
            }).RequireAuthorization();

        app.MapPut("/api/v1/track-location",
            async (UpdateTrackerForUserRequest request,
                HttpContext httpContext,
                UserAppointmentTrackerBusiness userAppointmentTrackerBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var updatedId =
                    await userAppointmentTrackerBusiness.UpdateTrackerForUser(request, userId);
                return Results.Ok(updatedId);
            }).RequireAuthorization();

        app.MapDelete("/api/v1/track-location/{locationTrackerId}",
            async (int locationTrackerId,
                HttpContext httpContext,
                UserAppointmentTrackerBusiness userAppointmentTrackerBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var deletedId = await userAppointmentTrackerBusiness.DeleteTrackerForUser(
                    locationTrackerId,
                    userId);
                return Results.Ok(deletedId);
            }).RequireAuthorization();
    }
}