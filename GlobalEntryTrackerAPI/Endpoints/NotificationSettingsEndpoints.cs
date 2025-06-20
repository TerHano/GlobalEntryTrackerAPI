using Business;
using Business.Dto;
using Business.Dto.NotificationSettings;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Models;

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
                })
            .RequireAuthorization()
            .WithTags("Notification Settings")
            .WithName("GetUserNotificationSettings")
            .WithSummary("Get all notification settings for the current user")
            .WithDescription(
                "Retrieves all notification settings for the currently authenticated user.")
            .Produces<ApiResponse<UserNotificationSettingsDto>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapGet("/api/v1/notification-settings/check",
                async (HttpContext httpContext, UserBusiness userBusiness) =>
                {
                    var userId = httpContext.User.GetUserId();

                    var notificationCheck =
                        await userBusiness.DoesUserHaveNotificationsSetUp(userId);
                    return Results.Ok(notificationCheck);
                })
            .RequireAuthorization()
            .WithTags("Notification Settings")
            .WithName("CheckUserNotificationSetup")
            .WithSummary("Check if user has notifications set up")
            .WithDescription(
                "Checks if the currently authenticated user has any notifications set up.")
            .Produces<ApiResponse<NotificationCheckDto>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}