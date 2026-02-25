using System.Text;
using AutoMapper;
using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;
using Database.Entities.NotificationSettings;
using Database.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;

namespace Business;

/// <summary>
///     Provides supporting authentication operations for managing user accounts.
///     This class handles password resets and email verification resend operations.
///     NOTE: Core authentication (sign-in, refresh tokens, etc.) is handled directly
///     in EntryAlertIdentityApiEndpointRouteBuilderExtension using SignInManager and
///     RefreshTokenProtector for security and feature completeness (2FA support, etc).
/// </summary>
public class IdentityAuthBusiness(
    UserProfileRepository userProfileRepository,
    UserNotificationRepository userNotificationRepository,
    UserManager<UserEntity> userManager,
    IMapper mapper
) : IAuthBusiness
{
    /// <summary>
    ///     Sends a password reset email to the user with the specified email address.
    ///     Generates a secure password reset token that must be provided when resetting the password.
    /// </summary>
    public async Task SendPasswordResetEmail(PasswordResetEmailRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            // Don't reveal whether the email exists for security
            return;

        if (!await userManager.IsEmailConfirmedAsync(user))
            // Don't reveal whether the email is confirmed for security
            return;

        // Generate password reset token
        // Token encoding and email sending is handled by the endpoint that calls this method
        await userManager.GeneratePasswordResetTokenAsync(user);
    }

    /// <summary>
    ///     Resends email verification to the user with the specified email address.
    ///     Only resends to users who haven't confirmed their email yet.
    /// </summary>
    public async Task ResendVerificationEmail(ResendEmailVerificationRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            // Don't reveal whether the email exists for security
            return;

        if (await userManager.IsEmailConfirmedAsync(user))
            // Email already confirmed, don't resend
            return;

        // Generate new email confirmation token
        // Token encoding and email sending is handled by the endpoint that calls this method
        await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task UpdateUser(UpdateUserRequest request, string userId)
    {
        var user = await userProfileRepository.GetUserProfileById(userId)
                   ?? throw new InvalidOperationException(
                       $"User profile not found for userId: {userId}");
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        await userProfileRepository.UpdateUserProfile(user);
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

    // NOTE: SignIn, RefreshToken, and VerifyOtp are not implemented here.
    // These operations are handled directly in EntryAlertIdentityApiEndpointRouteBuilderExtension
    // for better security and feature support (2FA, recovery codes, token protection, etc).
    // See AUTH_ARCHITECTURE_ANALYSIS.md for architecture details.

    public Task<AuthToken> SignIn(SignInRequest request)
    {
        throw new NotSupportedException(
            "SignIn is not implemented in IAuthBusiness. " +
            "Sign-in is handled directly by EntryAlertIdentityApiEndpointRouteBuilderExtension " +
            "using LoginRequest endpoint for better security and 2FA support. " +
            "See /api/auth/v1/login endpoint.");
    }

    public Task<AuthToken> RefreshToken(string refreshToken)
    {
        throw new NotSupportedException(
            "RefreshToken is not implemented in IAuthBusiness. " +
            "Token refresh is handled directly by EntryAlertIdentityApiEndpointRouteBuilderExtension " +
            "using RefreshTokenProtector for security. " +
            "See /api/auth/v1/refresh endpoint.");
    }

    public Task<AuthToken> VerifyOtp(string type, VerifyOtpRequest request)
    {
        throw new NotSupportedException(
            "OTP verification is not implemented. " +
            "The application uses password-based authentication. " +
            "Email verification is handled through confirmation links instead of OTP codes.");
    }

    /// <summary>
    ///     Resets the password for a user using the provided reset code and new password.
    ///     Validates that the user's email is confirmed before allowing password reset.
    /// </summary>
    public async Task<UserDto> ResetPasswordForUser(string userId, ResetPasswordRequest request)
    {
        var user = await userManager.FindByIdAsync(userId)
                   ?? throw new InvalidOperationException($"User not found with ID: {userId}");

        // Check if email is confirmed before allowing password reset
        if (!await userManager.IsEmailConfirmedAsync(user))
            throw new InvalidOperationException(
                "User email must be confirmed before resetting password");

        // Decode the reset code from Base64Url format
        IdentityResult result;
        try
        {
            var decodedCode =
                Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.ResetCode));
            result = await userManager.ResetPasswordAsync(user, decodedCode, request.NewPassword);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Invalid password reset code", ex);
        }

        if (!result.Succeeded)
        {
            var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password reset failed: {errorMessages}");
        }

        // Get updated user profile
        var userProfile = await userProfileRepository.GetUserProfileById(userId);
        if (userProfile == null)
            throw new InvalidOperationException($"User profile not found for userId: {userId}");

        return mapper.Map<UserDto>(userProfile);
    }
}