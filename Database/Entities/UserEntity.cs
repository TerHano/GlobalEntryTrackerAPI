using Microsoft.AspNetCore.Identity;

namespace Database.Entities;

public class UserEntity : IdentityUser
{
    public virtual ICollection<RoleEntity> UserRoles { get; set; }
    public virtual UserProfileEntity UserProfile { get; set; } = null!;
    public UserNotificationEntity UserNotification { get; set; } = null!;
    public UserCustomerEntity UserCustomer { get; set; } = null!;
}