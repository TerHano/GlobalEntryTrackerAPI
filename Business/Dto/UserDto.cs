using System.ComponentModel.DataAnnotations;
using Business.Dto.Admin;

namespace Business.Dto;

public class UserDto
{
    [Required] public string FirstName { get; set; }

    [Required] public string LastName { get; set; }

    [Required] public string Email { get; set; }

    [Required] public RoleDto Role { get; set; }
}