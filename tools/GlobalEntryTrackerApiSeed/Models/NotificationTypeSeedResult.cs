namespace GlobalEntryTrackerApiSeed.Models;

public class NotificationTypeSeedResult
{
    public bool Success { get; init; }

    public int Added { get; init; }

    public int Skipped { get; init; }

    public List<string> AddedTypes { get; init; } = [];

    public string? ErrorMessage { get; init; }
}