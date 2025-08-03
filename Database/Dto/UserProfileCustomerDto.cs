namespace Database.Entities;

public class UserProfileCustomerDto
{
    public UserEntity User { get; set; } = null!;
    public UserProfileEntity UserProfile { get; set; } = null!;
    public UserCustomerEntity UserCustomer { get; set; } = null!;
}