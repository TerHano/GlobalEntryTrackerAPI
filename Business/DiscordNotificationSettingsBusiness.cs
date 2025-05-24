using AutoMapper;
using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using Database.Entities.NotificationSettings;
using Database.Repositories;
using Service;
using Service.Enum;

namespace Business;

/// <summary>
///     Handles business logic for Discord notification settings.
/// </summary>
public class DiscordNotificationSettingsBusiness(
    UserNotificationRepository userNotificationRepository,
    NotificationManagerService notificationManager,
    IMapper mapper)
{
    /// <summary>
    ///     Gets the Discord notification settings for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>Discord notification settings DTO or null.</returns>
    public async Task<DiscordNotificationSettingsDto?> GetDiscordNotificationSettingsForUser(
        int userId)
    {
        var userNotification =
            await userNotificationRepository.GetUserWithNotificationSettings(userId);
        if (userNotification.DiscordNotificationSettings == null) return null;
        return mapper.Map<DiscordNotificationSettingsDto>(userNotification
            .DiscordNotificationSettings);
    }

    /// <summary>
    ///     Creates Discord notification settings for a user.
    /// </summary>
    /// <param name="settings">Settings request.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>ID of the created settings.</returns>
    public async Task<int> CreateDiscordNotificationSettingsForUser(
        CreateDiscordSettingsRequest settings, int userId)
    {
        var entity = mapper.Map<DiscordNotificationSettingsEntity>(settings);
        var id = await userNotificationRepository.UpdateUserDiscordNotificationSettings(userId,
            entity);
        return id;
    }

    /// <summary>
    ///     Updates Discord notification settings for a user.
    /// </summary>
    /// <param name="settings">Settings request.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>ID of the updated settings.</returns>
    public async Task<int> UpdateDiscordNotificationSettingsForUser(
        UpdateDiscordSettingsRequest settings, int userId)
    {
        var entity = mapper.Map<DiscordNotificationSettingsEntity>(settings);
        var id = await userNotificationRepository.UpdateUserDiscordNotificationSettings(userId,
            entity);
        return id;
    }

    /// <summary>
    ///     Sends a test Discord message using the provided settings.
    /// </summary>
    /// <param name="notificationSettings">Test settings request.</param>
    public async Task SendDiscordTestMessage(TestDiscordSettingsRequest notificationSettings)
    {
        await notificationManager.SendTestMessageForService(NotificationServiceType.Discord,
            notificationSettings);
    }
}