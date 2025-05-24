using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class RoleRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory)
{
    public async Task<List<RoleEntity>> GetAllRoles()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Roles.ToListAsync();
    }
}