using Database.Entities.NotificationSettings;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class DiscordNotificationSettingsRepository(GlobalEntryTrackerDbContext context)
{
    public async Task<DiscordNotificationSettingsEntity?> GetNotificationSettingsForUser(
        int userId)
    {
        var settings = await context.DiscordNotificationSettings.Where(x => x.UserId == userId)
            .FirstOrDefaultAsync();
        return settings;
    }
}