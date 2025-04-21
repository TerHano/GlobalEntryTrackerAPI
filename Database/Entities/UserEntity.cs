using System.ComponentModel.DataAnnotations;
using Database.Entities.NotificationSettings;

namespace Database.Entities;

public class UserEntity
{
    public int Id { get; init; }

    [MaxLength(256)] public required string ExternalId { get; set; }

    [MaxLength(254)] public required string Email { get; set; }

    [MaxLength(30)] public required string FirstName { get; set; }

    [MaxLength(40)] public required string LastName { get; set; }

    public required DateTime CreatedAt { get; init; }

    public int? DiscordNotificationSettingsId { get; init; }

    public DiscordNotificationSettingsEntity? DiscordNotificationSettings { get; init; }
    public virtual UserRoleEntity UserRole { get; set; }
}