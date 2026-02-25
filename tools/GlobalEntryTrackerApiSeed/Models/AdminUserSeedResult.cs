namespace GlobalEntryTrackerApiSeed.Models;

public class AdminUserSeedResult
{
    public bool Success { get; init; }

    public bool UserCreated { get; init; }

    public bool UserAlreadyExists { get; init; }

    public bool IsAlreadyAdmin { get; init; }

    public string? Email { get; init; }

    public string? UserId { get; init; }

    public string? Message { get; init; }

    public string? ErrorMessage { get; init; }
}