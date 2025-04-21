namespace GlobalEntryTrackerAPI.Models;

public class ApiResponse<T>
{
    public ApiResponse()
    {
        Success = true;
        ErrorMessages = [];
    }


    public ApiResponse(T data)
    {
        Success = true;
        Data = data;
        ErrorMessages = [];
    }


    public bool Success { get; set; } = true;
    public string[] ErrorMessages { get; set; }
    public T? Data { get; set; }
}