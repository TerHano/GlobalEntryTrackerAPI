using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class UserRepository(GlobalEntryTrackerDbContext context, ILogger<UserRepository> logger)
{
    public async Task CreateUser(UserEntity user)
    {
        try
        {
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
            //context.Users.Update(user);
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
        var user = await context.Users.Include(x => x.UserRole).ThenInclude(x => x.Role)
            .FirstAsync(x => x.Id == userId);
        if (user is null) throw new NullReferenceException("User does not exist");
        return user;
    }


    public async Task UpdateMultipleUsers(
        List<UserEntity> users)
    {
        try
        {
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
}