using Business;

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
            }).RequireAuthorization();
    }
}