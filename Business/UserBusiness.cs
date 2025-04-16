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

    public async Task UpdateUser(UpdateUserRequest request, int userId)
    {
        var user = await userRepository.GetUserById(userId);
        if (user == null) throw new Exception("User not found");
        mapper.Map(request, user);
        await userRepository.UpdateUser(user);
    }

    public async Task<UserDto> GetUserById(int userId)
    {
        var user = await userRepository.GetUserById(userId);
        return mapper.Map<UserDto>(user);
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