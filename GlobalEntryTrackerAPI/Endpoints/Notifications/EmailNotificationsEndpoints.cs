using Business;
using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Models;

namespace GlobalEntryTrackerAPI.Endpoints.Notifications;

public static class EmailNotificationEndpoints
{
    public static void MapEmailNotificationEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/notification-settings/email",
                async (HttpContext httpContext,
                    EmailNotificationSettingsBusiness emailNotificationSettingsBusiness) =>
                {
                    var userId = httpContext.User.GetUserId();
                    var emailSettings = await emailNotificationSettingsBusiness
                        .GetEmailNotificationSettingsForUser(userId);
                    return Results.Ok(emailSettings);
                })
            .RequireAuthorization()
            .WithTags("Email Notifications")
            .WithName("GetEmailNotificationSettings")
            .WithSummary("Get email notification settings for the current user")
            .WithDescription(
                "Retrieves the email notification settings for the currently authenticated user.")
            .Produces<ApiResponse<EmailNotificationSettingsDto>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapPost("/api/v1/notification-settings/email",
                async (CreateEmailNotificationSettingsRequest newSettings,
                    HttpContext httpContext,
                    EmailNotificationSettingsBusiness emailNotificationSettingsBusiness) =>
                {
                    var userId = httpContext.User.GetUserId();
                    var newSettingsId = await emailNotificationSettingsBusiness
                        .CreateEmailNotificationSettingsForUser(newSettings, userId);
                    return Results.Ok(newSettingsId);
                })
            .RequireAuthorization()
            .WithTags("Email Notifications")
            .WithName("CreateEmailNotificationSettings")
            .WithSummary("Create email notification settings for the current user")
            .WithDescription(
                "Creates new email notification settings for the currently authenticated user.")
            .Accepts<CreateEmailNotificationSettingsRequest>("application/json")
            .Produces<ApiResponse<int>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapPut("/api/v1/notification-settings/email",
                async (UpdateEmailNotificationSettingsRequest updatedSettings,
                    HttpContext httpContext,
                    EmailNotificationSettingsBusiness emailNotificationSettingsBusiness) =>
                {
                    var userId = httpContext.User.GetUserId();
                    var newSettingsId = await emailNotificationSettingsBusiness
                        .UpdateEmailNotificationSettingsForUser(updatedSettings, userId);
                    return Results.Ok(newSettingsId);
                })
            .RequireAuthorization()
            .WithTags("Email Notifications")
            .WithName("UpdateEmailNotificationSettings")
            .WithSummary("Update email notification settings for the current user")
            .WithDescription(
                "Updates the email notification settings for the currently authenticated user.")
            .Accepts<UpdateEmailNotificationSettingsRequest>("application/json")
            .Produces<ApiResponse<int>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapPost("/api/v1/notification-settings/email/test",
                async (HttpContext httpContext,
                    EmailNotificationSettingsBusiness emailNotificationSettingsBusiness) =>
                {
                    var userId = httpContext.User.GetUserId();
                    await emailNotificationSettingsBusiness
                        .SendEmailTestMessage(userId);
                    return Results.Ok();
                })
            .RequireAuthorization()
            .WithTags("Email Notifications")
            .WithName("SendTestEmailNotification")
            .WithSummary("Send a test email notification to the current user")
            .WithDescription(
                "Sends a test email notification to the currently authenticated user to verify email notification settings.")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}