using Database.Entities;

namespace Service;

public class UserRoleService
{
    public void UpdateNextNotificationTimeForUser(UserProfileEntity userProfileEntity,
        List<RoleEntity> roles)
    {
        var userRole = userProfileEntity.User.UserRoles?.FirstOrDefault();
        var roleInfo = roles.Find(x => x.Id == userRole?.Id);

        if (roleInfo == null)
            throw new InvalidOperationException(
                $"Role with ID '{userRole?.Id}' does not exist in the system");
        userProfileEntity.User.UserProfile.NextNotificationAt =
            DateTime.UtcNow.AddMinutes(roleInfo.NotificationIntervalInMinutes);
    }
}