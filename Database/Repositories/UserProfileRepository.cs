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

    /// <summary>
    ///     Persists the notification cooldown for a set of user profiles.
    ///     <see cref="UserProfileEntity.NextNotificationAt" /> is an internal, role-derived value and is
    ///     never supplied by a user, so this deliberately marks only that column as modified. Any other
    ///     in-memory changes on the supplied entities are ignored.
    /// </summary>
    public async Task UpdateNextNotificationTimes(List<UserProfileEntity> users)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            foreach (var user in users)
            {
                if (user is null) throw new NullReferenceException("User does not exist");
                var entry = context.UserProfiles.Attach(user);
                entry.Property(x => x.NextNotificationAt).IsModified = true;
            }

            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex.Message);
            throw new DbUpdateException("Failed to update next notification times", ex);
        }
    }

    public async Task DeleteUserProfile(string userId)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var user = await context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
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

    // public async Task<bool> IsUserAdmin(string userId)
    // {
    //     await using var context = await contextFactory.CreateDbContextAsync();
    //     var user = await context.Users
    //         .Include(x => x.UserRoles)
    //         .ThenInclude(x => x.Role)
    //         .FirstOrDefaultAsync(x => x.Id == userId);
    //     return user?.UserRoles?.Role.Code == Role.Admin.GetCode();
    // }

    /// <summary>
    ///     Gets all user profiles for admin view.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="includeSelf">Whether to include the current user in the results.</param>
    /// <returns>List of user entities.</returns>
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

    public async Task<List<string>> GetAllUserIds()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Users.Select(x => x.Id).ToListAsync();
    }
}