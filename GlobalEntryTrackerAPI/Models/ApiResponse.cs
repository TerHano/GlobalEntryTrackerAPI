using System.ComponentModel.DataAnnotations;

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


    [Required] public bool Success { get; set; }

    [Required] public List<Error> Errors { get; set; }

    [Required] public T Data { get; set; }
}