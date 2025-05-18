using Database.Entities;

namespace Service;

public class UserRoleService
{
    public int GetNextNotificationTimeForUser(UserEntity user)
    {
        if (user.UserRoles.Count == 0)
            return 0;

        var nextNotificationTime = user.UserRoles.Min(u => u.Role.NotificationIntervalInMinutes);

        return nextNotificationTime;
    }

    public int GetAllowedNumberOfTrackersForUser(UserEntity user)
    {
        if (user.UserRoles.Count == 0)
            return 20;

        var allowedNumberOfTrackers = user.UserRoles.Max(u => u.Role.MaxTrackers);

        return allowedNumberOfTrackers;
    }
}