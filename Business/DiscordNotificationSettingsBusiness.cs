using AutoMapper;
using Business.Dto.NotificationSettings;
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

    public async Task SendDiscordTestMessage(DiscordNotificationSettingsDto notificationSettings)
    {
        await notificationManager.SendTestMessageForService(NotificationServiceType.Discord,
            notificationSettings);
    }
}