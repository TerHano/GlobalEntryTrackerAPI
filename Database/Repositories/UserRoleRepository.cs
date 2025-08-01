using Database.Entities;
using Database.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) throw new NullReferenceException("User not found");
            if (!await userManager.IsInRoleAsync(user, nameof(role)))
                await userManager.AddToRoleAsync(user, nameof(role));
        }
        catch (DbUpdateException ex)
        {
            logger?.LogError(ex.Message);
            throw new DbUpdateException("Failed to create user role", ex);
        }
    }

    public async Task<List<RoleEntity>> GetUserRoleByUserId(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) throw new NullReferenceException("User not found");
        var roles = await userManager.GetRolesAsync(user);
        //get role entities from the database
        var roleEntities = await roleManager.Roles
            .Where(r => roles.Contains(r.Name))
            .ToListAsync();
        return roleEntities;
    }

    public async Task AddEditRoleForUser(string userId, Role role)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) throw new NullReferenceException("User not found");
            if (!await userManager.IsInRoleAsync(user, nameof(role)))
                await userManager.AddToRoleAsync(user, nameof(role));
        }
        catch (DbUpdateException ex)
        {
            logger?.LogError(ex.Message);
            throw new DbUpdateException("Failed to add role for user", ex);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            throw new Exception("Failed to add role for user", ex);
        }
    }

    public async Task RemoveRoleForUser(string userId, Role role)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) throw new NullReferenceException("User not found");
            if (await userManager.IsInRoleAsync(user, nameof(role)))
                await userManager.RemoveFromRoleAsync(user, nameof(role));
        }
        catch (DbUpdateException ex)
        {
            logger?.LogError(ex.Message);
            throw new DbUpdateException("Failed to remove role for user", ex);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            throw new Exception("Failed to add role for user", ex);
        }
    }
}