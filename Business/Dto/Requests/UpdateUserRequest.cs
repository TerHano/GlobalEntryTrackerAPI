using System.ComponentModel.DataAnnotations;

namespace Business.Dto.Requests;

public class UpdateUserRequest
{
    [Required] public string FirstName { get; set; }

    [Required] public string LastName { get; set; }
}