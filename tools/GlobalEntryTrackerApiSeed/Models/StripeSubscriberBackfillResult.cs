namespace GlobalEntryTrackerApiSeed.Models;

public class StripeSubscriberBackfillResult
{
    public bool Success { get; init; }

    public bool DryRun { get; init; }

    public int Processed { get; init; }

    public int Linked { get; init; }

    public int RoleAssignments { get; init; }

    public int Unmatched { get; init; }

    public string? Error { get; init; }
}