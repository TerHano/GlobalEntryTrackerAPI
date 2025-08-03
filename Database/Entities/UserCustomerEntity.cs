namespace Database.Entities;

public class UserCustomerEntity
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public UserEntity User { get; set; } = null!;
    public required string CustomerId { get; set; }
    public required string SubscriptionId { get; set; }
}