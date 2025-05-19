using Business;
using GlobalEntryTrackerAPI.Extensions;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class NotificationSettingsEndpoints
{
    public static void MapNotificationSettingsEndpoints(this WebApplication app)
    {
        // Get: api/v1/me/notifications Get All Notifications for User
        app.MapGet("/api/v1/notification-settings/",
            async (HttpContext httpContext, UserBusiness userBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var userNotificationSettings =
                    await userBusiness.GetAllNotificationsForUser(userId);
                return Results.Ok(userNotificationSettings);
            }).RequireAuthorization();
        app.MapGet("/api/v1/notification-settings/check",
            async (HttpContext httpContext, UserBusiness userBusiness) =>
            {
                var userId = httpContext.User.GetUserId();

                var notificationCheck = await userBusiness.DoesUserHaveNotificationsSetUp(userId);
                return Results.Ok(notificationCheck);
            }).RequireAuthorization();
    }
}