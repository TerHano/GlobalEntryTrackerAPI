using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class UserRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    ILogger<UserProfileRepository> logger)
{
    //Get All Users
    public async Task<UserEntity> GetUserById(string userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var user = await context.Users
            .Include(x => x.UserRoles)
            //.Include(x => x.UserProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null) throw new NullReferenceException("User does not exist");
        return user;
    }

    public async Task UpdateUser(UserEntity user)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update error while updating user");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while updating user");
            throw;
        }
    }
}