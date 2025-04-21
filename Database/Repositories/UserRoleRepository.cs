using Database.Entities;
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
}