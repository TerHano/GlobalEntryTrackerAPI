using AutoMapper;
using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using Database.Entities.NotificationSettings;
using Database.Repositories;
using Service;
using Service.Enum;

namespace Business;

public class DiscordNotificationSettingsBusiness(
    UserNotificationRepository userNotificationRepository,
    NotificationManagerService notificationManager,
    IMapper mapper)
{
    public async Task<DiscordNotificationSettingsDto?> GetDiscordNotificationSettingsForUser(
        int userId)
    {
        var userNotification =
            await userNotificationRepository.GetUserWithNotificationSettings(userId);
        if (userNotification.DiscordNotificationSettings == null) return null;
        return mapper.Map<DiscordNotificationSettingsDto>(userNotification
            .DiscordNotificationSettings);
    }

    public async Task<int> CreateDiscordNotificationSettingsForUser(
        CreateDiscordSettingsRequest settings, int userId)
    {
        var entity = mapper.Map<DiscordNotificationSettingsEntity>(settings);
        var id = await userNotificationRepository.UpdateUserDiscordNotificationSettings(userId,
            entity);
        return id;
    }

    public async Task<int> UpdateDiscordNotificationSettingsForUser(
        UpdateDiscordSettingsRequest settings, int userId)
    {
        var entity = mapper.Map<DiscordNotificationSettingsEntity>(settings);
        var id = await userNotificationRepository.UpdateUserDiscordNotificationSettings(userId,
            entity);
        return id;
    }

    public async Task SendDiscordTestMessage(TestDiscordSettingsRequest notificationSettings)
    {
        await notificationManager.SendTestMessageForService(NotificationServiceType.Discord,
            notificationSettings);
    }
}