using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class NotificationTypeRepository(GlobalEntryTrackerDbContext context)
{
    public async Task<List<NotificationTypeEntity>> GetAllNotificationTypes()
    {
        return await context.NotificationTypes.ToListAsync();
    }
}