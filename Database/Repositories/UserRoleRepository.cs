using Database.Entities;
using Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class UserRoleRepository(GlobalEntryTrackerDbContext context)
{
    public async Task CreateUserRole(UserRoleEntity userRole)
    {
        await context.UserRoles.AddAsync(userRole);
        await context.SaveChangesAsync();
    }

    public async Task<List<UserRoleEntity>> GetUserRolesByUserId(int userId)
    {
        var userRole = await context.UserRoles
            .Include(x => x.User)
            .Include(x => x.Role)
            .Where(x => x.UserId == userId).ToListAsync();
        if (userRole is null) throw new NullReferenceException("User role does not exist");
        return userRole;
    }

    public async Task AddRoleForUser(int userId, Role role)
    {
        await context.UserRoles.AddAsync(new UserRoleEntity
        {
            UserId = userId,
            RoleId = (int)role
        });
        await context.SaveChangesAsync();
    }

    //Remove role for user
    public async Task RemoveRoleForUser(int userId, Role role)
    {
        var existingUserRole = await context.UserRoles
            .Where(x => x.UserId == userId).Include(userRoleEntity => userRoleEntity.Role)
            .ToListAsync();
        if (existingUserRole == null) throw new NullReferenceException("User roles not found");
        var userRoleToRemove = existingUserRole.FirstOrDefault(x => x.Role.Id == (int)role);
        if (userRoleToRemove == null) throw new NullReferenceException("User role not found");
        context.UserRoles.Remove(userRoleToRemove);
        await context.SaveChangesAsync();
    }
}