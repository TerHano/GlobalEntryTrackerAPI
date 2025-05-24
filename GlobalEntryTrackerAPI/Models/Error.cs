using System.ComponentModel.DataAnnotations;

namespace GlobalEntryTrackerAPI.Models;

public class Error
{
    [Required] public int Code { get; set; }

    [Required] public string Message { get; set; } = string.Empty;
}