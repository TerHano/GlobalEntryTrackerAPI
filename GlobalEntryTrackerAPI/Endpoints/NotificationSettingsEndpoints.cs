using Business;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class NotificationSettingsEndpoints
{
    public static void MapNotificationSettingsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/notification-settings",
            async (NotificationBusiness notificationBusiness) =>
            {
                var notificationTypes = await notificationBusiness.GetNotificationAllTypes();
                return Results.Ok(notificationTypes);
            }).RequireAuthorization();
    }
}