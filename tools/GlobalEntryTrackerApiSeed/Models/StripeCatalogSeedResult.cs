namespace GlobalEntryTrackerApiSeed.Models;

public class StripeCatalogSeedResult
{
    public bool Success { get; init; }

    public bool DryRun { get; init; }

    public int MatchedRecurringPrices { get; init; }

    public int Created { get; init; }

    public int Updated { get; init; }

    public int SkippedMissingIdentifiers { get; init; }

    public string? Error { get; init; }
}