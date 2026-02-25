using Database.Entities;
using Database.Enums;

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
}