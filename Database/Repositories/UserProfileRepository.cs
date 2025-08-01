using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class UserProfileRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    ILogger<UserProfileRepository> logger)
{
    //Get All Users
    public async Task<List<UserProfileEntity>> GetAllUserProfiles()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.UserProfiles.Include(x => x.User).ThenInclude(x => x.UserRoles)
            .ToListAsync();
    }

    public async Task CreateUserProfile(UserProfileEntity userProfile)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            await context.UserProfiles.AddAsync(userProfile);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex.Message);
            throw new DbUpdateException("Failed to create user", ex);
        }
    }

    public async Task UpdateUserProfile(UserProfileEntity userProfile)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            context.UserProfiles.Update(userProfile);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex.Message);
            throw new DbUpdateException("Failed to update user", ex);
        }
    }

    public async Task<UserProfileEntity> GetUserProfileById(string userId, bool withRoles = false)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        UserProfileEntity? user;
        if (withRoles)
            user = await context.UserProfiles.Include(x => x.User).ThenInclude(x => x.UserRoles)
                .FirstOrDefaultAsync(x => x.UserId == userId);
        else
            user = await context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);

        if (user is null) throw new NullReferenceException("User does not exist");
        return user;
    }

    public async Task<List<UserProfileEntity>> GetUserProfileByIds(List<string> userId,
        bool withRoles = false)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        List<UserProfileEntity> users;
        if (withRoles)
            users = await context.UserProfiles.Include(x => x.User)
                .ThenInclude(x => x.UserRoles)
                .Where(x => userId.Contains(x.UserId)).ToListAsync();
        else
            users = await context.UserProfiles.Where(x => userId.Contains(x.UserId)).ToListAsync();
        return users;
    }

    public async Task UpdateMultipleUserProfiles(List<UserProfileEntity> users)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            foreach (var user in users) context.UserProfiles.Update(user);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex.Message);
            throw new DbUpdateException("Failed to update users", ex);
        }
    }

    public async Task DeleteUserProfile(string userId)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var user = await context.UserProfiles.FindAsync(userId);
            if (user is null) throw new NullReferenceException("User does not exist");
            context.UserProfiles.Remove(user);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex.Message);
            throw new DbUpdateException("Failed to delete user", ex);
        }
    }

    /// <summary>
    ///     Checks if the user has the admin role.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>True if the user is an admin, otherwise false.</returns>
    // public async Task<bool> IsUserAdmin(string userId)
    // {
    //     await using var context = await contextFactory.CreateDbContextAsync();
    //     var user = await context.Users
    //         .Include(x => x.UserRoles)
    //         .ThenInclude(x => x.Role)
    //         .FirstOrDefaultAsync(x => x.Id == userId);
    //     return user?.UserRoles?.Role.Code == Role.Admin.GetCode();
    // }

    //get users for admin
    public async Task<List<UserEntity>> GetAllUserProfilesForAdmin(string userId,
        bool includeSelf = false)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        List<UserEntity> users;
        if (!includeSelf)
            users = await context.Users.Include(x => x.UserProfile)
                .Include(x => x.UserRoles)
                .Include(x => x.UserCustomer)
                .Where(x => x.Id != userId).ToListAsync();
        else
            users = await context.Users.Include(x => x.UserProfile)
                .Include(x => x.UserRoles)
                .Include(x => x.UserCustomer)
                .ToListAsync();
        return users;
    }
}