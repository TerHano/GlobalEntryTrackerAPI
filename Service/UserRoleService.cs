using Database.Entities;

namespace Service;

public class UserRoleService
{
    public void UpdateNextNotificationTimeForUser(UserEntity user)
    {
        user.NextNotificationAt =
            DateTime.UtcNow.AddMinutes(user.UserRole.Role.NotificationIntervalInMinutes);
    }
}