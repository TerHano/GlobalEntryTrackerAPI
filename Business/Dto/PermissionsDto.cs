using System.ComponentModel.DataAnnotations;

namespace Business.Dto;

public class PermissionsDto
{
    [Required] public bool CanCreateTracker { get; set; }
}