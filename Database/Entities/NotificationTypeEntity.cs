using System.ComponentModel.DataAnnotations;

namespace Database.Entities;

public class NotificationTypeEntity
{
    public int Id { get; init; }

    [MaxLength(30)] public required string Name { get; init; }

    [MaxLength(256)] public required string Description { get; init; }
}