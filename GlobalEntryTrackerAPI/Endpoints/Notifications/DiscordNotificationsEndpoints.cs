using Business;
using Business.Dto.Requests;
using FluentValidation;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Validators;

namespace GlobalEntryTrackerAPI.Endpoints.Notifications;

public static class DiscordNotificationEndpoints
{
    public static void MapDiscordNotificationEndpoints(this WebApplication app)
    {
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