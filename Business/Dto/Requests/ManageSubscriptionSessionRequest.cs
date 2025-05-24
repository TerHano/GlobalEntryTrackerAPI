using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class ManageSubscriptionSessionRequest
{
    [Required] public string ReturnUrl { get; set; } = string.Empty;
}