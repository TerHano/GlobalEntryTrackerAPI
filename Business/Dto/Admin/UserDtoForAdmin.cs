using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Admin;

public class UserDtoForAdmin
{
    [Required] public int Id { get; init; }

    public required string ExternalId { get; set; }

    [Required] public required string Email { get; set; }

    [Required] public required string FirstName { get; set; }

    [Required] public required string LastName { get; set; }

    [Required] public required DateTime CreatedAt { get; init; }

    public virtual RoleDto Role { get; set; }

    public string CustomerId { get; set; } = null!;
    public string SubscriptionId { get; set; } = null!;

    [Required] public required DateTime NextNotificationAt { get; set; }
}