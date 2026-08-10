using Database.Entities;

namespace Service;

public class UserRoleService
{
    /// <summary>
    ///     Resolves the role backing a user profile, or null when the user has no known role.
    ///     Requires the profile to have been loaded with User.UserRoles.
    /// </summary>
    public RoleEntity? FindRoleForUser(UserProfileEntity userProfileEntity, List<RoleEntity> roles)
    {
        var userRole = userProfileEntity.User.UserRoles?.FirstOrDefault();
        return roles.Find(x => x.Id == userRole?.Id);
    }

    public void UpdateNextNotificationTimeForUser(UserProfileEntity userProfileEntity,
        List<RoleEntity> roles)
    {
        var roleInfo = FindRoleForUser(userProfileEntity, roles);
        if (roleInfo == null)
            throw new InvalidOperationException(
                $"No known role for user '{userProfileEntity.UserId}'");
        SetNextNotificationTimeForUser(userProfileEntity, roleInfo);
    }

    /// <summary>
    ///     Starts the post-notification cooldown for a user based on their role's interval.
    /// </summary>
    public void SetNextNotificationTimeForUser(UserProfileEntity userProfileEntity,
        RoleEntity roleInfo)
    {
        // Set on the profile that was passed in rather than round-tripping through
        // User.UserProfile, which only resolves back to this instance via EF relationship fixup.
        userProfileEntity.NextNotificationAt =
            DateTime.UtcNow.AddMinutes(roleInfo.NotificationIntervalInMinutes);
    }
}
