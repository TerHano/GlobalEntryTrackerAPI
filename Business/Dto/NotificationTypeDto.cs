using GlobalEntryTrackerAPI;

namespace Business.Dto;

public class NotificationTypeDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required NotificationType Type { get; init; }
}