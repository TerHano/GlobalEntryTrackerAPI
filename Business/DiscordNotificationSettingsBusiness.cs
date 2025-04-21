using AutoMapper;
using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using Database.Entities.NotificationSettings;
using Database.Repositories;
using Service;
using Service.Enum;

namespace Business;

public class DiscordNotificationSettingsBusiness(
    DiscordNotificationSettingsRepository discordNotificationSettingsRepository,
    NotificationManagerService notificationManager,
    IMapper mapper)
{
    public async Task<DiscordNotificationSettingsDto?> GetDiscordNotificationSettingsForUser(
        int userId)
    {
        var settings =
            await discordNotificationSettingsRepository.GetNotificationSettingsForUser(userId);
        if (settings is { Enabled: true })
            return mapper.Map<DiscordNotificationSettingsDto>(settings);
        return null;
    }

    public async Task<int> CreateDiscordNotificationSettingsForUser(
        CreateDiscordSettingsRequest settings, int userId)
    {
        var entity = mapper.Map<DiscordNotificationSettingsEntity>(settings);
        entity.UserId = userId;
        var entityId =
            await discordNotificationSettingsRepository.CreateNotificationSettingsForUser(entity);
        return entityId;
    }

    public async Task<int> UpdateDiscordNotificationSettingsForUser(
        UpdateDiscordSettingsRequest settings, int userId)
    {
        var entity = mapper.Map<DiscordNotificationSettingsEntity>(settings);
        entity.UserId = userId;
        var entityId =
            await discordNotificationSettingsRepository.UpdateNotificationSettingsForUser(entity);
        return entityId;
    }

    public async Task SendDiscordTestMessage(TestDiscordSettingsRequest notificationSettings)
    {
        await notificationManager.SendTestMessageForService(NotificationServiceType.Discord,
            notificationSettings);
    }
}