using Business.Dto;
using Business.Dto.Requests;
using Microsoft.AspNetCore.Identity.Data;

namespace Business;

public interface IAuthBusiness
{
    Task<AuthToken> SignIn(SignInRequest request);
    Task<AuthToken> RefreshToken(string refreshToken);
    Task SendPasswordResetEmail(PasswordResetEmailRequest request);
    Task<AuthToken> VerifyOtp(string type, VerifyOtpRequest request);
    Task ResendVerificationEmail(ResendEmailVerificationRequest request);
    Task<UserDto> ResetPasswordForUser(string userId, ResetPasswordRequest request);
    Task CreateUser(CreateUserRequest request, string userId);
    Task UpdateUser(UpdateUserRequest request, string userId);
    Task DeleteUserById(string userId);
}