using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class PasswordResetEmailRequest
{
    [Required] public string Email { get; set; }
    [Required] public string RedirectUrl { get; set; }
}