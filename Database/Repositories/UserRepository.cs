using Database.Entities;

namespace Database.Repositories;

public class UserRepository(GlobalEntryTrackerDbContext context)
{
    public async Task CreateUser(UserEntity user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }
}