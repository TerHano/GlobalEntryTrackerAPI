namespace GlobalEntryTrackerApiSeed.Models;

public class StripeCatalogSeedOptions
{
    public string? StripeSecretKey { get; init; }

    public HashSet<string> ProductIds { get; init; } = [];

    public HashSet<string> PriceIds { get; init; } = [];

    public bool OnlyActive { get; init; } = true;

    public bool DryRun { get; init; }
}