namespace Database.Enums;

public enum Role
{
    Free,
    Subscriber,
    Admin,
    FriendsFamily
}

public static class RoleExtensions
{
    public static string GetCode(this Role role)
    {
        return role switch
        {
            Role.Free => "free",
            Role.Subscriber => "subscriber",
            Role.Admin => "admin",
            Role.FriendsFamily => "friends_family",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}