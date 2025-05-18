using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class UserRepository(GlobalEntryTrackerDbContext context)
{
    public async Task CreateUser(UserEntity user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task UpdateUser(UserEntity user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task<UserEntity> GetUserById(int userId)
    {
        var user = await context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .FirstAsync(x => x.Id == userId);
        if (user is null) throw new NullReferenceException("User does not exist");
        return user;
    }

    public async Task<UserEntity> GetUserWithNotificationSettings(int userId)
    {
        var user = await context.Users.Include(x => x.DiscordNotificationSettings)
            .FirstAsync(user => user.Id == userId);
        if (user is null) throw new NullReferenceException("User does not exist");
        return user;
    }
}