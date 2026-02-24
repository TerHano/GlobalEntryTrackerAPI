namespace GlobalEntryTrackerApiSeed.Models;

public class LocationSeedResult
{
    public bool Success { get; init; }

    public bool DryRun { get; init; }

    public int FetchedFromApi { get; init; }

    public int ValidForDatabase { get; init; }

    public int Created { get; init; }

    public int Updated { get; init; }

    public string? Error { get; init; }
}