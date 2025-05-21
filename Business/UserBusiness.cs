using AutoMapper;
using Business.Dto;
using Business.Dto.NotificationSettings;
using Business.Dto.Requests;
using Database.Entities;
using Database.Entities.NotificationSettings;
using Database.Enums;
using Database.Repositories;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using Client = Supabase.Client;

namespace Business;

public class UserBusiness(
    TrackedLocationForUserRepository trackedLocationRepository,
    UserRepository userRepository,
    UserNotificationRepository userNotificationRepository,
    UserRoleRepository userRoleRepository,
    IMapper mapper)
{
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
        if (supabaseUser == null) throw new Exception("User could not be created");
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

    public async Task<UserDto> GetUserById(int userId)
    {
        var user = await userRepository.GetUserById(userId);
        return mapper.Map<UserDto>(user);
    }

    public async Task AssignRoleForUser(int userId, Role role)
    {
        var newUserRole = new UserRoleEntity
        {
            UserId = userId,
            RoleId = (int)role
        };
        await userRoleRepository.CreateUserRole(newUserRole);
    }

    public async Task<PermissionsDto> GetPermissionsForUser(int userId)
    {
        var trackersForUser = await trackedLocationRepository.GetTrackedLocationsForUser(userId);
        var numOfTrackers = trackersForUser.Count;
        var user = await userRepository.GetUserById(userId);
        var maxTrackers = user.UserRole.Role.MaxTrackers;
        var permissions = new PermissionsDto
        {
            CanCreateTracker = numOfTrackers < maxTrackers
        };
        return permissions;
    }


    public async Task<NotificationCheckDto> DoesUserHaveNotificationsSetUp(int userId)
    {
        var userNotification =
            await userNotificationRepository.GetUserWithNotificationSettings(userId);
        var isAnyNotificationsEnabled =
            userNotification?.EmailNotificationSettings?.Enabled == true ||
            userNotification?.DiscordNotificationSettings?.Enabled == true;
        return new NotificationCheckDto
        {
            IsAnyNotificationsEnabled = isAnyNotificationsEnabled
        };
    }

    public async Task<UserNotificationSettingsDto> GetAllNotificationsForUser(int userId)
    {
        var user = await userNotificationRepository.GetUserWithNotificationSettings(userId);
        var userNotificationSettingsDto = new UserNotificationSettingsDto
        {
            DiscordSettings =
                mapper.Map<DiscordNotificationSettingsDto>(user.DiscordNotificationSettings),
            EmailSettings =
                mapper.Map<EmailNotificationSettingsDto>(user.EmailNotificationSettings)
        };
        return userNotificationSettingsDto;
    }

    public async Task DeleteUserById(int userId)
    {
        var user = await userRepository.GetUserById(userId);
        var supabaseAdminClient = await GetSupabaseAdminClient();
        var response = await supabaseAdminClient.DeleteUser(user.ExternalId);
        if (response == null) throw new Exception("User could not be deleted");
        await userRepository.DeleteUser(userId);
    }

    private async Task<Client> GetSupabaseClient()
    {
        var supabaseUrl = Environment.GetEnvironmentVariable("Auth__SupabaseUrl");
        var supabaseKey = Environment.GetEnvironmentVariable("Auth__SupabaseAnonKey");
        var supabaseClient = new Client(supabaseUrl, supabaseKey);
        await supabaseClient.InitializeAsync();
        return supabaseClient;
    }

    private async Task<IGotrueAdminClient<User>> GetSupabaseAdminClient()
    {
        var supabaseServiceKey = Environment.GetEnvironmentVariable("Auth__SupabaseServiceKey");

        var supabaseClient = await GetSupabaseClient();
        return supabaseClient.AdminAuth(supabaseServiceKey);
    }
}