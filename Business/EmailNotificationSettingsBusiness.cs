using AutoMapper;
using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using Database.Entities.NotificationSettings;
using Database.Repositories;
using Service;
using Service.Enum;

namespace Business;

public class EmailNotificationSettingsBusiness(
    EmailNotificationSettingsRepository emailNotificationSettingsRepository,
    UserNotificationRepository userNotificationRepository,
    NotificationManagerService notificationManager,
    UserRepository userRepository,
    IMapper mapper)
{
    public async Task<EmailNotificationSettingsDto?> GetEmailNotificationSettingsForUser(
        int userId)
    {
        var userNotification =
            await userNotificationRepository.GetUserWithNotificationSettings(userId);
        if (userNotification.EmailNotificationSettings == null) return null;
        return mapper.Map<EmailNotificationSettingsDto>(userNotification.EmailNotificationSettings);
    }

    public async Task<int> CreateEmailNotificationSettingsForUser(
        CreateEmailNotificationSettingsRequest settings, int userId)
    {
        var userNotification =
            await userNotificationRepository.GetUserWithNotificationSettings(userId);
        var entity = mapper.Map<EmailNotificationSettingsEntity>(settings);
        userNotification.EmailNotificationSettings = entity;
        await userNotificationRepository.UpdateUserNotification(userNotification);
        return userNotification.EmailNotificationSettings.Id;
    }

    public async Task<int> UpdateEmailNotificationSettingsForUser(
        UpdateEmailNotificationSettingsRequest settings, int userId)
    {
        var userNotification =
            await userNotificationRepository.GetUserWithNotificationSettings(userId);
        if (userNotification.EmailNotificationSettingsId != settings.Id)
            throw new UnauthorizedAccessException("Cannot update settings");
        var entity = mapper.Map<EmailNotificationSettingsEntity>(settings);
        userNotification.EmailNotificationSettings = entity;
        await userNotificationRepository.UpdateUserNotification(userNotification);
        return userNotification.EmailNotificationSettings.Id;
    }

    public async Task SendEmailTestMessage(int userId)
    {
        var user = await userRepository.GetUserById(userId);
        if (user == null)
            throw new Exception($"User with ID {userId} not found.");
        await notificationManager.SendTestMessageForService(NotificationServiceType.Email,
            user.Email);
    }
}