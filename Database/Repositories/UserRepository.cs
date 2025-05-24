using Database.Entities;
using Database.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class UserRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    ILogger<UserRepository> logger)
{
    //Get All Users
    public async Task<List<UserEntity>> GetAllUsers()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Users.Include(x => x.UserRole).ThenInclude(x => x.Role).ToListAsync();
    }

    public async Task CreateUser(UserEntity user)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex.Message);
            throw new DbUpdateException("Failed to create user", ex);
        }
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
            logger.LogError(ex.Message);
            throw new DbUpdateException("Failed to update user", ex);
        }
    }

    public async Task<UserEntity> GetUserById(int userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var user = await context.Users.Include(x => x.UserRole).ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null) throw new NullReferenceException("User does not exist");
        return user;
    }

    public async Task UpdateMultipleUsers(List<UserEntity> users)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            foreach (var user in users) context.Users.Update(user);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex.Message);
            throw new DbUpdateException("Failed to update users", ex);
        }
    }

    public async Task DeleteUser(int userId)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var user = await context.Users.FindAsync(userId);
            if (user is null) throw new NullReferenceException("User does not exist");
            context.Users.Remove(user);
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
    public async Task<bool> IsUserAdmin(int userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var user = await context.Users
            .Include(x => x.UserRole)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId);
        return user?.UserRole?.Role.Code == Role.Admin.GetCode();
    }

    //get users for admin
    public async Task<List<UserEntity>> GetAllUsersForAdmin()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Users.Include(x => x.UserRole).ThenInclude(x => x.Role)
            .Include(x => x.UserCustomer)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();
    }
}