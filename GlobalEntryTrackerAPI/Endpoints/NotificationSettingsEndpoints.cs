using Business;
using Business.Dto.NotificationSettings;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Models;
using Service.Dto;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class NotificationSettingsEndpoints
{
    public static void MapNotificationSettingsEndpoints(this WebApplication app)
    {
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
                return Results.Ok(
                    new ApiResponse<DiscordNotificationSettingsDto?>(discordSettings));
            }).RequireAuthorization();
        app.MapPost("/api/v1/notification-settings/discord",
            async (DiscordNotificationSettingsDto newSettings,
                HttpContext httpContext,
                DiscordNotificationSettingsBusiness discordNotificationSettingsBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var newSettingsId = await discordNotificationSettingsBusiness
                    .CreateDiscordNotificationSettingsForUser(newSettings, userId);
                return Results.Ok(
                    newSettingsId);
            }).RequireAuthorization();

        app.MapPut("/api/v1/notification-settings/discord",
            async (DiscordNotificationSettingsDto updatedSettings,
                HttpContext httpContext,
                DiscordNotificationSettingsBusiness discordNotificationSettingsBusiness) =>
            {
                var userId = httpContext.User.GetUserId();
                var newSettingsId = await discordNotificationSettingsBusiness
                    .UpdateDiscordNotificationSettingsForUser(updatedSettings, userId);
                return Results.Ok(
                    newSettingsId);
            }).RequireAuthorization();

        app.MapPost("/api/v1/notification-settings/discord/test",
            async (TestDiscordNotificationDto testSettings,
                DiscordNotificationSettingsBusiness discordNotificationSettingsBusiness) =>
            {
                await discordNotificationSettingsBusiness
                    .SendDiscordTestMessage(testSettings);
                return Results.Ok();
            }).RequireAuthorization();
    }
}