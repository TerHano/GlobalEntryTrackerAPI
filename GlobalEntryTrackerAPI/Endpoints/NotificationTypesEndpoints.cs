using Business;
using Business.Dto;
using GlobalEntryTrackerAPI.Models;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class NotificationTypesEndpoints
{
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/notification-types",
                async (NotificationBusiness notificationBusiness) =>
                {
                    var notificationTypes = await notificationBusiness.GetNotificationAllTypes();
                    return Results.Ok(notificationTypes);
                })
            .RequireAuthorization()
            .WithTags("Notification Types")
            .WithName("GetNotificationTypes")
            .WithSummary("Get all notification types")
            .WithDescription(
                "Retrieves a list of all available notification types for the authenticated user.")
            .Produces<ApiResponse<NotificationTypeDto[]>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}