using AutoMapper;
using Business.Dto;
using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using Database.Entities;
using Database.Enums;
using Database.Repositories;
using Service;

namespace Business;

public class UserBusiness(
    TrackedLocationForUserRepository trackedLocationRepository,
    UserRepository userRepository,
    UserRoleRepository userRoleRepository,
    UserRoleService userRoleService,
    IMapper mapper)
{
    public async Task CreateUser(CreateUserRequest request)
    {
        var newUser = mapper.Map<UserEntity>(request);
        await userRepository.CreateUser(newUser);
        var newUserRole = new UserRoleEntity
        {
            UserId = newUser.Id,
            RoleId = (int)Role.Free
        };
        await userRoleRepository.CreateUserRole(newUserRole);
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

    public async Task AssignRoleForUser(int userId, Role role)
    {
        var newUserRole = new UserRoleEntity
        {
            UserId = userId,
            RoleId = (int)role
        };
        await userRoleRepository.CreateUserRole(newUserRole);
    }

    public async Task<PermissionsDto> GetPermissionsForUser(int userId)
    {
        var trackersForUser = await trackedLocationRepository.GetTrackedLocationsForUser(userId);
        var numOfTrackers = trackersForUser.Count;
        var user = await userRepository.GetUserById(userId);
        var maxTrackers = userRoleService.GetAllowedNumberOfTrackersForUser(user);
        var permissions = new PermissionsDto
        {
            CanCreateTracker = numOfTrackers < maxTrackers
        };
        return permissions;
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

    public async Task<UserNotificationSettingsDto> GetAllNotificationsForUser(int userId)
    {
        var user = await userRepository.GetUserWithNotificationSettings(userId);
        var userNotificationSettingsDto = new UserNotificationSettingsDto
        {
            DiscordSettings =
                mapper.Map<DiscordNotificationSettingsDto>(user.DiscordNotificationSettings)
        };
        return userNotificationSettingsDto;
    }
}