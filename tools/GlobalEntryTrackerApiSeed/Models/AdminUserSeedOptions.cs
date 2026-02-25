namespace GlobalEntryTrackerApiSeed.Models;

public class AdminUserSeedOptions
{
    public required string Email { get; init; }

    public required string Password { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public bool DryRun { get; init; }
}