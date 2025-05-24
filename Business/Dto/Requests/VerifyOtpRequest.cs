using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class VerifyOtpRequest
{
    [Required] public string TokenHash { get; set; }
}