using AutoMapper;
using Business.Dto;
using Business.Dto.Admin;
using Business.Dto.NotificationSettings;
using Database.Entities;
using Database.Enums;
using Database.Repositories;
using Quartz;

namespace Business;

/// <summary>
///     Handles business logic for user management and user-related operations.
/// </summary>
public class UserBusiness(
    TrackedLocationForUserRepository trackedLocationRepository,
    UserRepository userRepository,
    UserNotificationRepository userNotificationRepository,
    UserRoleRepository userRoleRepository,
    ISchedulerFactory schedulerFactory,
    IMapper mapper)
{
    //get all users for admin
    public async Task<List<UserDtoForAdmin>> GetAllUsersForAdmin(int userId,
        bool includeSelf = false)
    {
        var users = await userRepository.GetAllUsersForAdmin(userId, includeSelf);
        return mapper.Map<List<UserDtoForAdmin>>(users);
    }


    /// <summary>
    ///     Gets a user by their ID.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>User DTO.</returns>
    public async Task<UserDto> GetUserById(int userId)
    {
        var user = await userRepository.GetUserById(userId);
        return mapper.Map<UserDto>(user);
    }

    /// <summary>
    ///     Gets the next notification check time for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>DateTime of the next notification check, or null.</returns>
    public async Task<DateTime?> GetNextNotificationCheckForUser(int userId)
    {
        var user = await userRepository.GetUserById(userId);
        var activeTrackersForUser =
            await trackedLocationRepository.GetTrackedLocationsForUser(userId);
        if (activeTrackersForUser.Count == 0 ||
            activeTrackersForUser.All(x => x.Enabled == false))
            return null;
        var nextNotificationCheck = user.NextNotificationAt;
        return nextNotificationCheck.AddMinutes(5);
    }

    /// <summary>
    ///     Assigns a role to a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="role">Role to assign.</param>
    public async Task AssignRoleForUser(int userId, Role role)
    {
        var newUserRole = new UserRoleEntity
        {
            UserId = userId,
            RoleId = (int)role
        };
        await userRoleRepository.CreateUserRole(newUserRole);
    }

    /// <summary>
    ///     Gets permissions for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>Permissions DTO.</returns>
    public async Task<PermissionsDto> GetPermissionsForUser(int userId)
    {
        var trackersForUser = await trackedLocationRepository.GetTrackedLocationsForUser(userId);
        var numOfTrackers = trackersForUser.Count;
        var user = await userRepository.GetUserById(userId);
        var maxTrackers = user.UserRole.Role.MaxTrackers;
        var permissions = new PermissionsDto
        {
            CanCreateTracker = numOfTrackers < maxTrackers
        };
        return permissions;
    }

    /// <summary>
    ///     Checks if the user has any notifications set up.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>Notification check DTO.</returns>
    public async Task<NotificationCheckDto> DoesUserHaveNotificationsSetUp(int userId)
    {
        var userNotification =
            await userNotificationRepository.GetUserWithNotificationSettings(userId);
        var isAnyNotificationsEnabled =
            userNotification?.EmailNotificationSettings?.Enabled == true ||
            userNotification?.DiscordNotificationSettings?.Enabled == true;
        return new NotificationCheckDto
        {
            IsAnyNotificationsEnabled = isAnyNotificationsEnabled
        };
    }

    /// <summary>
    ///     Gets all notification settings for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>User notification settings DTO.</returns>
    public async Task<UserNotificationSettingsDto> GetAllNotificationsForUser(int userId)
    {
        var user = await userNotificationRepository.GetUserWithNotificationSettings(userId);
        var userNotificationSettingsDto = new UserNotificationSettingsDto
        {
            DiscordSettings =
                mapper.Map<DiscordNotificationSettingsDto>(user.DiscordNotificationSettings),
            EmailSettings =
                mapper.Map<EmailNotificationSettingsDto>(user.EmailNotificationSettings)
        };
        return userNotificationSettingsDto;
    }


    /// <summary>
    ///     Checks if the user has the admin role.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>True if the user is an admin, otherwise false.</returns>
    public async Task<bool> IsUserAdmin(int userId)
    {
        return await userRepository.IsUserAdmin(userId);
    }
}