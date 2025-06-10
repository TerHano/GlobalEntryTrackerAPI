namespace Business.Dto;

public class AuthToken
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required string Domain { get; set; }
}