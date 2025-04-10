namespace GlobalEntryTrackerAPI.Models;

public class ApiResponse
{
    public ApiResponse()
    {
        Success = true;
        ErrorMessage = string.Empty;
    }

    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}