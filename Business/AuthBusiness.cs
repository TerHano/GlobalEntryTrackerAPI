using System.Text;
using System.Text.Json;
using AutoMapper;
using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;
using Database.Entities.NotificationSettings;
using Database.Enums;
using Database.Repositories;
using Microsoft.Extensions.Configuration;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using Client = Supabase.Client;

namespace Business;

public class AuthBusiness(
    UserRepository userRepository,
    UserRoleRepository userRoleRepository,
    UserNotificationRepository userNotificationRepository,
    HttpClient httpClient,
    IConfiguration configuration,
    IMapper mapper
)
{
    /// <summary>
    ///     Sign in a user and return a token.
    /// </summary>
    /// <param name="request">User sign in request</param>
    public async Task<AuthToken> SignIn(SignInRequest request)
    {
        var supabaseClient = await GetSupabaseClient();
        var response =
            await supabaseClient.Auth.SignInWithPassword(request.Email, request.Password);
        if (response?.AccessToken == null || response?.RefreshToken == null)
            throw new Exception("User could not be signed in");
        return new AuthToken
        {
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken
        };
    }

    public async Task<AuthToken> RefreshToken(string refreshToken)
    {
        return await RefreshTokenThroughSupabase(refreshToken);
    }

    public async Task SendPasswordResetEmail(PasswordResetEmailRequest request)
    {
        var supabaseClient = await GetSupabaseClient();
        try
        {
            var success = await supabaseClient.Auth.ResetPasswordForEmail(
                new ResetPasswordForEmailOptions(request.Email)
                {
                    RedirectTo = request.RedirectUrl
                });
        }
        catch (Exception ex)
        {
            throw new Exception("Password reset email failed to send", ex);
        }
    }

    public async Task<AuthToken> VerifyOtp(Constants.EmailOtpType type, VerifyOtpRequest request)
    {
        var supabaseClient = await GetSupabaseClient();
        var session =
            await supabaseClient.Auth.VerifyTokenHash(request.TokenHash, type);
        if (session == null) throw new Exception("User could not be verified");
        if (session.AccessToken == null || session.RefreshToken == null)
            throw new Exception("User could not be verified");
        return new AuthToken
        {
            AccessToken = session.AccessToken,
            RefreshToken = session.RefreshToken
        };
    }

    public async Task<UserDto> ResetPasswordForUser(int userId, ResetPasswordRequest request)
    {
        var user = await userRepository.GetUserById(userId);
        if (user == null) throw new Exception("User not found");
        var externalId = user.ExternalId;
        try
        {
            var supabaseAdminClient = await GetSupabaseAdminClient();
            var userUpdate = await supabaseAdminClient.UpdateUserById(
                externalId,
                new AdminUserAttributes
                {
                    Password = request.NewPassword
                });
            if (userUpdate == null) throw new Exception("User could not be updated");
            var userEntity = mapper.Map<UserDto>(user);
            return userEntity;
        }
        catch (Exception ex)
        {
            throw new Exception("User could not be updated", ex);
        }
    }


    public async Task CreateUser(CreateUserRequest request)
    {
        var supabaseClient = await GetSupabaseClient();
        var signUpOptions = new SignUpOptions
        {
            RedirectTo = request.RedirectUrl,
            Data = new Dictionary<string, object>
            {
                { "first_name", request.FirstName }, { "last_name", request.LastName }
            }
        };
        var response =
            await supabaseClient.Auth.SignUp(request.Email, request.Password, signUpOptions);
        var supabaseUser = response?.User;
        if (supabaseUser?.Id == null) throw new Exception("User could not be created");
        var newUser = new UserEntity
        {
            Id = 0,
            ExternalId = supabaseUser.Id,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
            NextNotificationAt = DateTime.UtcNow
        };
        await userRepository.CreateUser(newUser);
        var newUserRole = new UserRoleEntity
        {
            UserId = newUser.Id,
            RoleId = (int)Role.Free
        };
        await userRoleRepository.CreateUserRole(newUserRole);
        var notificationId = await userNotificationRepository.CreateUserNotification(newUser.Id);
        await userNotificationRepository.UpdateUserEmailNotificationSettings(newUser.Id,
            new EmailNotificationSettingsEntity
            {
                UserNotificationId = notificationId,
                Email = request.Email,
                Enabled = false
            });
    }

    /// <summary>
    ///     Updates user information.
    /// </summary>
    /// <param name="request">User update request.</param>
    /// <param name="userId">User ID.</param>
    public async Task UpdateUser(UpdateUserRequest request, int userId)
    {
        var user = await userRepository.GetUserById(userId);
        if (user == null) throw new Exception("User not found");
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        await userRepository.UpdateUser(user);
        var supabaseAdminClient = await GetSupabaseAdminClient();
        await supabaseAdminClient.UpdateUserById(user.ExternalId,
            new AdminUserAttributes
            {
                UserMetadata = new Dictionary<string, object>
                {
                    { "first_name", request.FirstName },
                    { "last_name", request.LastName }
                }
            });
        mapper.Map(request, user);
    }

    /// <summary>
    ///     Deletes a user by their ID.
    /// </summary>
    /// <param name="userId">User ID.</param>
    public async Task DeleteUserById(int userId)
    {
        var user = await userRepository.GetUserById(userId);
        var supabaseAdminClient = await GetSupabaseAdminClient();
        await supabaseAdminClient.DeleteUser(user.ExternalId);
        await userRepository.DeleteUser(userId);
    }

    /// <summary>
    ///     Gets a Supabase client instance.
    /// </summary>
    /// <returns>Supabase client.</returns>
    private async Task<Client> GetSupabaseClient()
    {
        var supabaseUrl = configuration["Auth:Supabase_Url"];
        var supabaseKey = configuration["Auth:Supabase_Anon_Key"];
        if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
            throw new ApplicationException(
                "Supabase URL or key is not set in environment variables or configuration");
        var supabaseClient = new Client(supabaseUrl, supabaseKey);
        await supabaseClient.InitializeAsync();
        return supabaseClient;
    }

    private async Task<AuthToken> RefreshTokenThroughSupabase(string refreshToken)
    {
        var payload = new { refresh_token = refreshToken };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var supabaseUrl = configuration["Auth:Supabase_Url"];
        var supabaseKey = configuration["Auth:Supabase_Anon_Key"];

        var requestMessage = new HttpRequestMessage(HttpMethod.Post,
            $"{supabaseUrl}/auth/v1/token?grant_type=refresh_token")
        {
            Content = content
        };
        requestMessage.Headers.Add("apiKey", supabaseKey);

        var response = await httpClient.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to refresh token through Supabase");
        var responseBody = await response.Content.ReadAsStringAsync();
        var supabaseRefreshBody =
            JsonSerializer.Deserialize<SupabaseRefreshTokenResponse>(responseBody);
        if (supabaseRefreshBody == null)
            throw new Exception("Failed to deserialize Supabase refresh token response");
        var accessToken = supabaseRefreshBody.AccessToken;
        var refreshTokenResp = supabaseRefreshBody.RefreshToken;
        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshTokenResp))
            throw new Exception("Failed to refresh token through Supabase");
        return new AuthToken
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenResp
        };
        // Use accessToken and refreshTokenResp as needed
    }

    /// <summary>
    ///     Gets a Supabase admin client instance.
    /// </summary>
    /// <returns>Supabase admin client.</returns>
    private async Task<IGotrueAdminClient<User>> GetSupabaseAdminClient()
    {
        var supabaseServiceKey = configuration["Auth:Supabase_Service_Key"];
        if (string.IsNullOrEmpty(supabaseServiceKey))
            throw new ApplicationException(
                "Supabase service key is not set in environment variables or configuration");
        var supabaseClient = await GetSupabaseClient();
        return supabaseClient.AdminAuth(supabaseServiceKey);
    }
}