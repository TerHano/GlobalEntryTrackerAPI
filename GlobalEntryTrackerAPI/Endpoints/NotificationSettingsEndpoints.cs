using Business;
using Business.Dto.Requests;
using FluentValidation;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Validators;

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


        app.MapGet("/api/v1/notification-settings/discord",
            async (HttpContext httpContext,
                DiscordNotificationSettingsBusiness discordNotificationSettingsBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var discordSettings = await discordNotificationSettingsBusiness
                    .GetDiscordNotificationSettingsForUser(userId);
                return Results.Ok(discordSettings);
            }).RequireAuthorization();
        app.MapPost("/api/v1/notification-settings/discord",
            async (CreateDiscordSettingsRequest newSettings,
                HttpContext httpContext,
                DiscordNotificationSettingsBusiness discordNotificationSettingsBusiness) =>
            {
                var validator = new CreateDiscordSettingsRequestValidator();
                await validator.ValidateAndThrowAsync(newSettings);
                var userId = httpContext.User.GetUserId();
                var newSettingsId = await discordNotificationSettingsBusiness
                    .CreateDiscordNotificationSettingsForUser(newSettings, userId);
                return Results.Ok(
                    newSettingsId);
            }).RequireAuthorization();

        app.MapPut("/api/v1/notification-settings/discord",
            async (UpdateDiscordSettingsRequest updatedSettings,
                HttpContext httpContext,
                DiscordNotificationSettingsBusiness discordNotificationSettingsBusiness) =>
            {
                var validator = new UpdateDiscordSettingsRequestValidator();
                await validator.ValidateAndThrowAsync(updatedSettings);
                var userId = httpContext.User.GetUserId();
                var newSettingsId = await discordNotificationSettingsBusiness
                    .UpdateDiscordNotificationSettingsForUser(updatedSettings, userId);
                return Results.Ok(
                    newSettingsId);
            }).RequireAuthorization();

        app.MapPost("/api/v1/notification-settings/discord/test",
            async (TestDiscordSettingsRequest testSettings,
                DiscordNotificationSettingsBusiness discordNotificationSettingsBusiness) =>
            {
                var validator = new TestDiscordSettingsRequestValidator();
                await testSettings.ValidateRequestAsync(validator);
                await discordNotificationSettingsBusiness
                    .SendDiscordTestMessage(testSettings);
                return Results.Ok();
            }).RequireAuthorization();
    }
}