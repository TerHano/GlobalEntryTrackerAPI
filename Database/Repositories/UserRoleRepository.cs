using Database.Entities;
using Database.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class UserRoleRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    ILogger<UserRoleRepository> logger)
{
    public async Task CreateUserRole(UserRoleEntity userRole)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger?.LogError(ex.Message);
            throw new DbUpdateException("Failed to create user role", ex);
        }
    }

    public async Task<List<UserRoleEntity>> GetUserRolesByUserId(int userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var userRole = await context.UserRoles
            .Include(x => x.User)
            .Include(x => x.Role)
            .Where(x => x.UserId == userId).ToListAsync();
        if (userRole is null) throw new NullReferenceException("User role does not exist");
        return userRole;
    }

    public async Task AddEditRoleForUser(int userId, Role role)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var existingUserRole = await context.UserRoles
                .Where(x => x.UserId == userId).FirstOrDefaultAsync();
            if (existingUserRole == null)
            {
                var newUserRole = new UserRoleEntity
                {
                    UserId = userId,
                    RoleId = (int)role
                };
                await context.UserRoles.AddAsync(newUserRole);
            }
            else
            {
                existingUserRole.RoleId = (int)role;
                context.UserRoles.Update(existingUserRole);
            }

            await context.SaveChangesAsync();
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

    public async Task RemoveRoleForUser(int userId, Role role)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var existingUserRole = await context.UserRoles
                .Where(x => x.UserId == userId).Include(userRoleEntity => userRoleEntity.Role)
                .FirstOrDefaultAsync(x => x.Role.Id == (int)role);
            if (existingUserRole == null) throw new NullReferenceException("User roles not found");
            context.UserRoles.Remove(existingUserRole);
            await context.SaveChangesAsync();
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