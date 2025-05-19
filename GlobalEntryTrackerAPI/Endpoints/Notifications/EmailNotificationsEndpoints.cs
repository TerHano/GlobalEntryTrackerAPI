using Business;
using Business.Dto.Requests;
using GlobalEntryTrackerAPI.Extensions;

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
            }).RequireAuthorization();
        app.MapPost("/api/v1/notification-settings/email",
            async (CreateEmailNotificationSettingsRequest newSettings,
                HttpContext httpContext,
                EmailNotificationSettingsBusiness emailNotificationSettingsBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var newSettingsId = await emailNotificationSettingsBusiness
                    .CreateEmailNotificationSettingsForUser(newSettings, userId);
                return Results.Ok(
                    newSettingsId);
            }).RequireAuthorization();

        app.MapPut("/api/v1/notification-settings/email",
            async (UpdateEmailNotificationSettingsRequest updatedSettings,
                HttpContext httpContext,
                EmailNotificationSettingsBusiness emailNotificationSettingsBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var newSettingsId = await emailNotificationSettingsBusiness
                    .UpdateEmailNotificationSettingsForUser(updatedSettings, userId);
                return Results.Ok(
                    newSettingsId);
            }).RequireAuthorization();

        app.MapPost("/api/v1/notification-settings/email/test",
            async (HttpContext httpContext,
                EmailNotificationSettingsBusiness emailNotificationSettingsBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                await emailNotificationSettingsBusiness
                    .SendEmailTestMessage(userId);
                return Results.Ok();
            }).RequireAuthorization();
    }
}