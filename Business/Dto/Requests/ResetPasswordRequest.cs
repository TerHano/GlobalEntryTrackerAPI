using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class ResetPasswordRequest
{
    [Required] public string NewPassword { get; set; }
}