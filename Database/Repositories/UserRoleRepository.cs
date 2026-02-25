using Database.Entities;
using Database.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Database.Repositories;

public class UserRoleRepository(
    UserManager<UserEntity> userManager,
    RoleManager<RoleEntity> roleManager,
    ILogger<UserRoleRepository> logger)
{
    public async Task CreateUserRole(string userId, Role role)
    {
        try
        {
            var user = await GetUserOrThrow(userId);
            var roleName = role.ToString();
            if (!await userManager.IsInRoleAsync(user, roleName))
                await userManager.AddToRoleAsync(user, roleName);
        }
        catch (DbUpdateException ex)
        {
            logger?.LogError(ex.Message);
            throw new DbUpdateException("Failed to create user role", ex);
        }
    }

    public async Task<List<RoleEntity>> GetUserRoleByUserId(string userId)
    {
        var user = await GetUserOrThrow(userId);
        var roles = await userManager.GetRolesAsync(user);
        var roleEntities = await roleManager.Roles
            .Where(r => roles.Contains(r.Name))
            .ToListAsync();
        return roleEntities;
    }

    public async Task AddEditRoleForUser(string userId, Role role)
    {
        try
        {
            var user = await GetUserOrThrow(userId);
            var roleName = role.ToString();

            // GetRolesAsync always queries the store, so no re-fetch is needed to bust
            // any in-memory cache on the entity instance.
            var currentRoles = await userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
                await userManager.RemoveFromRolesAsync(user, currentRoles);


            if (!await userManager.IsInRoleAsync(user, roleName))
                await userManager.AddToRoleAsync(user, roleName);
        }
        catch (DbUpdateException ex) when (IsDuplicateRoleAssignment(ex))
        {
            // A concurrent request already assigned the same role (e.g. checkout.session.completed
            // and invoice.paid arriving simultaneously). This is safe to ignore.
            logger?.LogWarning(
                "Concurrent duplicate role assignment for role {Role} on user {UserId} — ignoring",
                role, userId);
        }
        catch (DbUpdateException ex)
        {
            logger?.LogError(ex, "Database error adding role {Role} to user {UserId}", role,
                userId);
            throw new InvalidOperationException($"Failed to add role {role} for user {userId}", ex);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Unexpected error adding role {Role} to user {UserId}", role,
                userId);
            throw new InvalidOperationException(
                $"Unexpected error adding role {role} for user {userId}", ex);
        }
    }

    public async Task RemoveRoleForUser(string userId, Role role)
    {
        try
        {
            var user = await GetUserOrThrow(userId);
            var roleName = role.ToString();
            if (await userManager.IsInRoleAsync(user, roleName))
                await userManager.RemoveFromRoleAsync(user, roleName);
        }
        catch (DbUpdateException ex)
        {
            logger?.LogError(ex, "Database error removing role {Role} from user {UserId}", role,
                userId);
            throw new InvalidOperationException($"Failed to remove role {role} for user {userId}",
                ex);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Unexpected error removing role {Role} from user {UserId}", role,
                userId);
            throw new InvalidOperationException(
                $"Unexpected error removing role {role} for user {userId}", ex);
        }
    }

    private async Task<UserEntity> GetUserOrThrow(string userId)
    {
        return await userManager.FindByIdAsync(userId)
               ?? throw new InvalidOperationException($"User not found with ID: {userId}");
    }

    private static bool IsDuplicateRoleAssignment(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException postgresException &&
               postgresException.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}