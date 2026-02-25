namespace GlobalEntryTrackerApiSeed.Models;

public class RoleSeedResult
{
    public bool Success { get; init; }

    public int Added { get; init; }

    public int Skipped { get; init; }

    public List<string> AddedRoles { get; init; } = [];

    public string? ErrorMessage { get; init; }
}

