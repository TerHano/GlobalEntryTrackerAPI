using Database.Entities;
using Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace GlobalEntryTrackerAPI.Util;

public static class SeedUtil
{
    public static List<RoleEntity> GetRoles()
    {
        var freeRole = new RoleEntity(nameof(Role.Free), Role.Free.GetCode(), 2, 300);
        var subscriberRole =
            new RoleEntity(nameof(Role.Subscriber), Role.Subscriber.GetCode(), 5, 1000);
        var adminRole = new RoleEntity(nameof(Role.Admin), Role.Admin.GetCode(), 10, 1);
        var friendsFamilyRole = new RoleEntity(nameof(Role.FriendsFamily),
            Role.FriendsFamily.GetCode(), 10, 1);
        return
        [
            freeRole,
            subscriberRole,
            adminRole,
            friendsFamilyRole
        ];
    }

    private static List<NotificationTypeEntity> GetNotificationTypes()
    {
        var weekendNotificationType =
            new NotificationTypeEntity
            {
                Name = "Weekend",
                Description = "Notify about weekend entries",
                Type = NotificationType.Weekends
            };
        var soonestNotificationType =
            new NotificationTypeEntity
            {
                Name = "Soonest",
                Description = "Notify about soonest entries",
                Type = NotificationType.Soonest
            };
        return
        [
            weekendNotificationType,
            soonestNotificationType
        ];
    }


    public static void Seed(DbContext context)
    {
        var anyNotificationTypes =
            context.Set<UserNotificationEntity>().Any();
        if (!anyNotificationTypes)
        {
            var notificationTypes = GetNotificationTypes();
            context.Set<NotificationTypeEntity>().AddRange(notificationTypes);
        }

        context.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var anyNotificationTypes =
            await context.Set<UserNotificationEntity>().AnyAsync(cancellationToken);
        if (!anyNotificationTypes)
        {
            var notificationTypes = GetNotificationTypes();
            await context.Set<NotificationTypeEntity>()
                .AddRangeAsync(notificationTypes, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}