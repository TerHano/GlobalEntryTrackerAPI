namespace GlobalEntryTrackerAPI.Models;

public class ApiResponse<T> : ApiResponse
{
    public ApiResponse(T data)
    {
        Success = true;
        ErrorMessage = string.Empty;
        Data = data;
    }

    public T? Data { get; set; }
}