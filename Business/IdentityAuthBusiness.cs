using AutoMapper;
using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;
using Database.Entities.NotificationSettings;
using Database.Repositories;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Configuration;

namespace Business;

public class IdentityAuthBusiness(
    UserProfileRepository userProfileRepository,
    UserRoleRepository userRoleRepository,
    UserNotificationRepository userNotificationRepository,
    HttpClient httpClient,
    IConfiguration configuration,
    IMapper mapper
) : IAuthBusiness
{
    public Task<AuthToken> SignIn(SignInRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<AuthToken> RefreshToken(string refreshToken)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetEmail(PasswordResetEmailRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<AuthToken> VerifyOtp(string type, VerifyOtpRequest request)
    {
        throw new NotImplementedException();
    }

    public Task ResendVerificationEmail(ResendEmailVerificationRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<UserDto> ResetPasswordForUser(string userId, ResetPasswordRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateUser(UpdateUserRequest request, string userId)
    {
        var user = await userProfileRepository.GetUserProfileById(userId);
        if (user == null) throw new Exception("User profile not found");
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        await userProfileRepository.UpdateUserProfile(user);
        mapper.Map(request, user);
    }

    public async Task DeleteUserById(string userId)
    {
        await userProfileRepository.DeleteUserProfile(userId);
    }

    public async Task CreateUser(CreateUserRequest request, string userId)
    {
        var newUser = new UserProfileEntity
        {
            Id = 0,
            UserId = userId,
            NextNotificationAt = DateTime.UtcNow,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow
        };
        await userProfileRepository.CreateUserProfile(newUser);
        var notificationId = await userNotificationRepository.CreateUserNotification(userId);
        var notification = await userNotificationRepository.GetById(notificationId);
        await userNotificationRepository.UpdateUserEmailNotificationSettings(userId,
            new EmailNotificationSettingsEntity
            {
                UserNotification = notification,
                Email = request.Email,
                Enabled = false
            });
    }
}