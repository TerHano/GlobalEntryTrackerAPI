using System.ComponentModel.DataAnnotations;

namespace Database.Entities;

public class UserProfileEntity
{
    public int Id { get; init; }
    public required string UserId { get; init; } = null!;
    public UserEntity User { get; init; } = null!;
    [MaxLength(254)] public required string Email { get; set; }

    [MaxLength(30)] public required string FirstName { get; set; }

    [MaxLength(40)] public required string LastName { get; set; }
    public required DateTime CreatedAt { get; init; }

    public DateTime? NextNotificationAt { get; set; }
}