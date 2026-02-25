using Database;
using Database.Entities;
using Database.Enums;
using GlobalEntryTrackerApiSeed.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GlobalEntryTrackerApiSeed.Services;

public class AdminUserSeederService(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    UserManager<UserEntity> userManager,
    RoleManager<RoleEntity> roleManager,
    ILogger<AdminUserSeederService> logger)
{
    public async Task<AdminUserSeedResult> SeedAdminUserAsync(AdminUserSeedOptions options)
    {
        logger.LogInformation("Starting admin user seeding (dry-run={DryRun})", options.DryRun);

        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(options.Email))
            {
                logger.LogError("Admin email is required");
                return new AdminUserSeedResult
                {
                    Success = false,
                    ErrorMessage = "Admin email is required"
                };
            }

            if (string.IsNullOrWhiteSpace(options.Password))
            {
                logger.LogError("Admin password is required");
                return new AdminUserSeedResult
                {
                    Success = false,
                    ErrorMessage = "Admin password is required"
                };
            }

            // Check if admin role exists
            var adminRole = await roleManager.FindByNameAsync(nameof(Role.Admin));
            if (adminRole == null)
            {
                logger.LogError("Admin role does not exist. Please run role seeding first.");
                return new AdminUserSeedResult
                {
                    Success = false,
                    ErrorMessage = "Admin role does not exist in the database"
                };
            }

            // Check if user already exists
            var existingUser = await userManager.FindByEmailAsync(options.Email);
            if (existingUser != null)
            {
                logger.LogInformation("Admin user with email '{Email}' already exists",
                    options.Email);

                // Check if user is already an admin
                var isAdmin = await userManager.IsInRoleAsync(existingUser, nameof(Role.Admin));

                return new AdminUserSeedResult
                {
                    Success = true,
                    UserAlreadyExists = true,
                    IsAlreadyAdmin = isAdmin,
                    Email = options.Email,
                    UserId = existingUser.Id
                };
            }

            // Create the admin user if not in dry-run mode
            if (options.DryRun)
            {
                logger.LogInformation("DRY RUN: Would create admin user with email '{Email}'",
                    options.Email);
                return new AdminUserSeedResult
                {
                    Success = true,
                    UserCreated = true,
                    Email = options.Email,
                    Message = "Dry run - admin user would be created"
                };
            }

            // Create the user
            var newUser = new UserEntity
            {
                UserName = options.Email,
                Email = options.Email,
                EmailConfirmed = true // Auto-confirm admin email
            };

            var createResult = await userManager.CreateAsync(newUser, options.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to create admin user: {Errors}", errors);
                return new AdminUserSeedResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to create admin user: {errors}"
                };
            }

            logger.LogInformation(
                "Successfully created admin user with email '{Email}' and ID '{UserId}'",
                options.Email, newUser.Id);

            // Assign admin role
            var roleResult = await userManager.AddToRoleAsync(newUser, nameof(Role.Admin));

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to assign admin role to user: {Errors}", errors);
                return new AdminUserSeedResult
                {
                    Success = false,
                    ErrorMessage = $"User created but failed to assign admin role: {errors}",
                    UserId = newUser.Id
                };
            }

            logger.LogInformation("Successfully assigned Admin role to user '{Email}'",
                options.Email);

            // Create user profile
            await using var context = await contextFactory.CreateDbContextAsync();

            var userProfile = new UserProfileEntity
            {
                UserId = newUser.Id,
                Email = options.Email,
                FirstName = options.FirstName ?? "Admin",
                LastName = options.LastName ?? "User",
                CreatedAt = DateTime.UtcNow,
                NextNotificationAt = DateTime.UtcNow
            };

            context.UserProfiles.Add(userProfile);

            // Create user notification settings
            var userNotification = new UserNotificationEntity
            {
                UserId = newUser.Id
            };

            context.UserNotifications.Add(userNotification);

            await context.SaveChangesAsync();

            logger.LogInformation(
                "Successfully created user profile and notification settings for admin user");

            return new AdminUserSeedResult
            {
                Success = true,
                UserCreated = true,
                Email = options.Email,
                UserId = newUser.Id,
                Message = "Admin user created successfully"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding admin user");
            return new AdminUserSeedResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}