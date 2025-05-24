using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class NotificationTypeRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory)
{
    public async Task<List<NotificationTypeEntity>> GetAllNotificationTypes()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.NotificationTypes.ToListAsync();
    }
}