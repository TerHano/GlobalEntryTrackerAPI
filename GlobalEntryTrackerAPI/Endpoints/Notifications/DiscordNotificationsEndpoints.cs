using Business;
using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using FluentValidation;
using GlobalEntryTrackerAPI.Extensions;
using GlobalEntryTrackerAPI.Models;
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
                })
            .RequireAuthorization()
            .WithTags("Discord Notifications")
            .WithName("GetDiscordNotificationSettings")
            .WithSummary("Get Discord notification settings for the current user")
            .WithDescription(
                "Retrieves the Discord notification settings for the currently authenticated user.")
            .Produces<ApiResponse<DiscordNotificationSettingsDto>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

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
                    return Results.Ok(newSettingsId);
                })
            .RequireAuthorization()
            .WithTags("Discord Notifications")
            .WithName("CreateDiscordNotificationSettings")
            .WithSummary("Create Discord notification settings for the current user")
            .WithDescription(
                "Creates new Discord notification settings for the currently authenticated user.")
            .Accepts<CreateDiscordSettingsRequest>("application/json")
            .Produces<ApiResponse<int>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

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
                    return Results.Ok(newSettingsId);
                })
            .RequireAuthorization()
            .WithTags("Discord Notifications")
            .WithName("UpdateDiscordNotificationSettings")
            .WithSummary("Update Discord notification settings for the current user")
            .WithDescription(
                "Updates the Discord notification settings for the currently authenticated user.")
            .Accepts<UpdateDiscordSettingsRequest>("application/json")
            .Produces<ApiResponse<int>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        app.MapPost("/api/v1/notification-settings/discord/test",
                async (TestDiscordSettingsRequest testSettings,
                    DiscordNotificationSettingsBusiness discordNotificationSettingsBusiness) =>
                {
                    var validator = new TestDiscordSettingsRequestValidator();
                    await testSettings.ValidateRequestAsync(validator);
                    await discordNotificationSettingsBusiness
                        .SendDiscordTestMessage(testSettings);
                    return Results.Ok();
                })
            .RequireAuthorization()
            .WithTags("Discord Notifications")
            .WithName("SendTestDiscordNotification")
            .WithSummary("Send a test Discord notification to the current user")
            .WithDescription(
                "Sends a test Discord notification to the currently authenticated user to verify Discord notification settings.")
            .Accepts<TestDiscordSettingsRequest>("application/json")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}