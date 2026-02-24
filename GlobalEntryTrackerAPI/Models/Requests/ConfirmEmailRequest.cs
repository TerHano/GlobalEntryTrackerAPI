namespace GlobalEntryTrackerAPI.Models.Requests;

public class ConfirmEmailRequest
{
    public string UserId { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? ChangedEmail { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}