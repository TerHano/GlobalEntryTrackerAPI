namespace Database.Enums;

public enum Role
{
    Free = 0,
    Subscriber = 1,
    Admin = 2
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
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}