using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class CreateUserRequest
{
    [Required] public string FirstName { get; set; }

    [Required] public string LastName { get; set; }

    [Required] public string Email { get; set; }

    [Required] public string Password { get; set; }

    [Required] public string RedirectUrl { get; set; } = string.Empty;
}