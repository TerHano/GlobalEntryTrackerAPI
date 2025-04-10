using AutoMapper;
using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;
using Database.Repositories;

namespace Business;

public class UserBusiness(UserRepository userRepository, IMapper mapper)
{
    public async Task CreateUser(CreateUserRequest request)
    {
        var newUser = mapper.Map<UserEntity>(request);
        await userRepository.CreateUser(newUser);
    }

    public async Task<NotificationCheckDto> DoesUserHaveNotificationsSetUp(int userId)
    {
        var user = await userRepository.GetUserWithNotificationSettings(userId);
        var isNotificationsSetUp = false;
        var isAnyNotificationsEnabled = false;
        if (user.DiscordNotificationSettingsId != null) isNotificationsSetUp = true;
        if (user.DiscordNotificationSettings is { Enabled: true }) isAnyNotificationsEnabled = true;
        return new NotificationCheckDto
        {
            IsNotificationsSetUp = isNotificationsSetUp,
            isAnyNotificationsEnabled = isAnyNotificationsEnabled
        };
    }
}