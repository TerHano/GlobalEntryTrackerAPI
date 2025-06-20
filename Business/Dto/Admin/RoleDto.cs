using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Admin;

public class RoleDto
{
    [Required] public int Id { get; set; }

    [Required] public string Name { get; set; }

    [Required] public string Code { get; set; } = null!;
}