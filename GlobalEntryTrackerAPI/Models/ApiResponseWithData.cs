namespace GlobalEntryTrackerAPI.Models;

public class ApiResponse<T>
{
    public ApiResponse()
    {
        Success = true;
        ErrorMessage = string.Empty;
    }

    public ApiResponse(string errorMessage)
    {
        Success = false;
        ErrorMessage = errorMessage;
    }

    public ApiResponse(T data)
    {
        Success = true;
        Data = data;
        ErrorMessage = string.Empty;
    }


    public bool Success { get; set; } = true;
    public string ErrorMessage { get; set; } = string.Empty;
    public T? Data { get; set; }
}