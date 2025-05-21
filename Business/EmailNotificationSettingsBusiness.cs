using AutoMapper;
using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using Database.Entities.NotificationSettings;
using Database.Repositories;
using Service;
using Service.Enum;

namespace Business;

public class EmailNotificationSettingsBusiness(
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
        var user = await userRepository.GetUserById(userId);
        var entity = mapper.Map<EmailNotificationSettingsEntity>(settings);
        entity.Email = user.Email;
        var id = await userNotificationRepository.UpdateUserEmailNotificationSettings(userId,
            entity);
        return id;
    }

    public async Task<int> UpdateEmailNotificationSettingsForUser(
        UpdateEmailNotificationSettingsRequest settings, int userId)
    {
        var entity = mapper.Map<EmailNotificationSettingsEntity>(settings);
        var user = await userRepository.GetUserById(userId);
        entity.Email = user.Email;
        var id = await userNotificationRepository.UpdateUserEmailNotificationSettings(userId,
            entity);
        return id;
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