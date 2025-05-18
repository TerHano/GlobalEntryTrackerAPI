using Database.Enums;

namespace Business.Dto;

public class UserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public List<Role> Roles { get; set; }
    public string NextNotificationAt { get; set; }
}