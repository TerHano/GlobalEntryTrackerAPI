namespace GlobalEntryTrackerAPI.Models;

public class ApiResponse<T>
{
    public ApiResponse()
    {
        Success = true;
        Errors = [];
    }


    public ApiResponse(T data)
    {
        Success = true;
        Data = data;
        Errors = [];
    }


    public bool Success { get; set; }
    public List<Error> Errors { get; set; }
    public T? Data { get; set; }
}