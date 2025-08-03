using AutoMapper;
using Business.Dto;
using Business.Dto.Admin;
using Business.Dto.NotificationSettings;
using Database.Entities;
using Database.Enums;
using Database.Repositories;
using Microsoft.AspNetCore.Identity;
using Quartz;

namespace Business;

/// <summary>
///     Handles business logic for user management and user-related operations.
/// </summary>
public class UserBusiness(
    TrackedLocationForUserRepository trackedLocationRepository,
    UserProfileRepository userProfileRepository,
    UserNotificationRepository userNotificationRepository,
    UserRoleRepository userRoleRepository,
    ISchedulerFactory schedulerFactory,
    UserManager<UserEntity> userManager,
    IMapper mapper)
{
    //get all users for admin
    public async Task<List<UserDtoForAdmin>> GetAllUsersForAdmin(string userId,
        bool includeSelf = false)
    {
        var users = await userProfileRepository.GetAllUserProfilesForAdmin(userId, includeSelf);
        return mapper.Map<List<UserDtoForAdmin>>(users);
    }


    /// <summary>
    ///     Gets a user by their ID.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>User DTO.</returns>
    public async Task<UserDto> GetUserById(string userId)
    {
        var user = await userProfileRepository.GetUserProfileById(userId, true);
        return mapper.Map<UserDto>(user);
    }

    /// <summary>
    ///     Gets the next notification check time for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>DateTime of the next notification check, or null.</returns>
    public async Task<DateTime?> GetNextNotificationCheckForUser(string userId)
    {
        var userProfileEntity = await userProfileRepository.GetUserProfileById(userId);
        if (userProfileEntity == null)
            throw new NullReferenceException("User Profile does not exist");
        var activeTrackersForUser =
            await trackedLocationRepository.GetTrackedLocationsForUser(userId);
        if (activeTrackersForUser.Count == 0 ||
            activeTrackersForUser.All(x => x.Enabled == false))
            return null;
        var nextNotificationCheck = userProfileEntity.NextNotificationAt;
        return nextNotificationCheck?.AddMinutes(5);
    }

    /// <summary>
    ///     Assigns a role to a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="role">Role to assign.</param>
    public async Task AssignRoleForUser(string userId, Role role)
    {
        await userRoleRepository.CreateUserRole(userId, role);
    }

    /// <summary>
    ///     Gets permissions for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>Permissions DTO.</returns>
    public async Task<PermissionsDto> GetPermissionsForUser(string userId)
    {
        var trackersForUser = await trackedLocationRepository.GetTrackedLocationsForUser(userId);
        var numOfTrackers = trackersForUser.Count;
        var userRoles = await userRoleRepository.GetUserRoleByUserId(userId);
        var maxTrackers = userRoles.Max(x => x.MaxTrackers);
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
    public async Task<NotificationCheckDto> DoesUserHaveNotificationsSetUp(string userId)
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
    public async Task<UserNotificationSettingsDto> GetAllNotificationsForUser(string userId)
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


    // /// <summary>
    // ///     Checks if the user has the admin role.
    // /// </summary>
    // /// <param name="userId">User ID.</param>
    // /// <returns>True if the user is an admin, otherwise false.</returns>
    // public async Task<bool> IsUserAdmin(string userId)
    // {
    //     return await userProfileRepository.IsUserAdmin(userId);
    // }
}