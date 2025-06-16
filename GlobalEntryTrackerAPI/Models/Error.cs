using System.ComponentModel.DataAnnotations;
using Business.Enum;

namespace GlobalEntryTrackerAPI.Models;

public class Error
{
    [Required] public ExceptionCode Code { get; set; }

    [Required] public string Message { get; set; } = string.Empty;
}