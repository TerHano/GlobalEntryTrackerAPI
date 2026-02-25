namespace GlobalEntryTrackerApiSeed.Models;

public class SeedReport
{
    public DateTime GeneratedAtUtc { get; init; }

    public bool DryRun { get; init; }

    public LocationSeedResult? LocationSeed { get; init; }

    public StripeCatalogSeedResult? StripeCatalogSeed { get; init; }

    public StripeSubscriberBackfillResult? StripeSubscriberBackfill { get; init; }

    public NotificationTypeSeedResult? NotificationTypeSeed { get; init; }

    public AdminUserSeedResult? AdminUserSeed { get; init; }
}

public class ReportOutputOptions
{
    public string ReportPath { get; init; } = "seed-report.json";
}