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

    public async Task<int> CreateNotificationSettingsForUser(
        DiscordNotificationSettingsEntity entity)
    {
        var newSettings = await context.DiscordNotificationSettings.AddAsync(entity);
        await context.SaveChangesAsync();
        return newSettings.Entity.Id;
    }

    public async Task<int> UpdateNotificationSettingsForUser(
        DiscordNotificationSettingsEntity entity)
    {
        var existingSettings = await context.DiscordNotificationSettings
            .FirstOrDefaultAsync(x => x.UserId == entity.UserId);
        if (existingSettings == null) throw new NullReferenceException("Settings not found");
        if (existingSettings.UserId != entity.UserId)
            throw new UnauthorizedAccessException(
                "You are not authorized to update these notification settings.");
        context.Entry(existingSettings).CurrentValues.SetValues(entity);
        await context.SaveChangesAsync();
        return existingSettings.Id;
    }
}