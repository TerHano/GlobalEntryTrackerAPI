using System.ComponentModel.DataAnnotations;
using GlobalEntryTrackerAPI;

namespace Business.Dto;

public class NotificationTypeDto
{
    [Required] public int Id { get; init; }

    [Required] public required string Name { get; init; }

    [Required] public required string Description { get; init; }

    [Required] public required NotificationType Type { get; init; }
}