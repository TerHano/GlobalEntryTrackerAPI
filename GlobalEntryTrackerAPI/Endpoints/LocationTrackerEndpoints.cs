using Business;
using Business.Dto;
using Business.Dto.Requests;
using FluentValidation;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Models;
using GlobalEntryTrackerAPI.Validators;

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
                    await userAppointmentTrackerBusiness.GetTrackedAppointmentLocationsForUser(
                        userId);
                return Results.Ok(trackedLocations);
            })
            .RequireAuthorization()
            .WithTags("Location Tracker")
            .WithName("GetTrackedLocations")
            .WithSummary("Get all tracked locations for the current user")
            .WithDescription(
                "Retrieves all appointment locations tracked by the currently authenticated user.")
            .Produces<ApiResponse<TrackedLocationForUserDto[]>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapGet("/api/v1/tracked-locations/{id}", async (int id, HttpContext httpContext,
                UserAppointmentTrackerBusiness userAppointmentTrackerBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var trackedLocations =
                    await userAppointmentTrackerBusiness.GetTrackedAppointmentLocationById(id,
                        userId);
                return Results.Ok(trackedLocations);
            })
            .RequireAuthorization()
            .WithTags("Location Tracker")
            .WithName("GetTrackedLocationById")
            .WithSummary("Get a tracked location by ID")
            .WithDescription(
                "Retrieves a specific tracked appointment location by its ID for the current user.")
            .Produces<ApiResponse<TrackedLocationForUserDto>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapPost("/api/v1/track-location",
                async (CreateTrackerForUserRequest request,
                    HttpContext httpContext,
                    UserAppointmentTrackerBusiness userAppointmentTrackerBusiness) =>
                {
                    // Validate the request
                    var validator = new CreateTrackerForUserRequestValidator();
                    await validator.ValidateAndThrowAsync(request);
                    var userId = httpContext.User.GetUserId();
                    var createdId =
                        await userAppointmentTrackerBusiness.CreateTrackerForUser(request, userId);
                    return Results.Created($"/api/v1/tracked-locations/{createdId}", createdId);
                })
            .RequireAuthorization()
            .WithTags("Location Tracker")
            .WithName("CreateTrackedLocation")
            .WithSummary("Create a new tracked location")
            .WithDescription("Creates a new tracked appointment location for the current user.")
            .Accepts<CreateTrackerForUserRequest>("application/json")
            .Produces<ApiResponse<int>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapPut("/api/v1/track-location",
                async (UpdateTrackerForUserRequest request,
                    HttpContext httpContext,
                    UserAppointmentTrackerBusiness userAppointmentTrackerBusiness) =>
                {
                    var validator = new UpdateTrackerForUserRequestValidator();
                    await validator.ValidateAndThrowAsync(request);
                    var userId = httpContext.User.GetUserId();
                    var updatedId =
                        await userAppointmentTrackerBusiness.UpdateTrackerForUser(request, userId);
                    return Results.Ok(updatedId);
                })
            .RequireAuthorization()
            .WithTags("Location Tracker")
            .WithName("UpdateTrackedLocation")
            .WithSummary("Update a tracked location")
            .WithDescription(
                "Updates an existing tracked appointment location for the current user.")
            .Accepts<UpdateTrackerForUserRequest>("application/json")
            .Produces<ApiResponse<int>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

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
                })
            .RequireAuthorization()
            .WithTags("Location Tracker")
            .WithName("DeleteTrackedLocationById")
            .WithSummary("Delete a tracked location by ID")
            .WithDescription(
                "Deletes a specific tracked appointment location by its ID for the current user.")
            .Produces<ApiResponse<int>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapDelete("/api/v1/track-location",
                async (HttpContext httpContext,
                    UserAppointmentTrackerBusiness userAppointmentTrackerBusiness) =>
                {
                    var userId = httpContext.User.GetUserId();
                    await userAppointmentTrackerBusiness.DeleteAllTrackersForUser(userId);
                    return Results.Ok();
                })
            .RequireAuthorization()
            .WithTags("Location Tracker")
            .WithName("DeleteAllTrackedLocations")
            .WithSummary("Delete all tracked locations for the current user")
            .WithDescription(
                "Deletes all tracked appointment locations for the currently authenticated user.")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}