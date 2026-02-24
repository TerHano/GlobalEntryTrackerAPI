namespace GlobalEntryTrackerApiSeed.Models;

public class StripeSubscriberBackfillOptions
{
    public string? StripeSecretKey { get; init; }

    public HashSet<string> SubscriptionStatuses { get; init; } =
        ["active", "trialing"];

    public bool MatchByEmail { get; init; } = true;

    public bool DryRun { get; init; }
}