using Database.Entities.NotificationSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class DiscordNotificationSettingsRepository(
    GlobalEntryTrackerDbContext context,
    ILogger<DiscordNotificationSettingsRepository> logger)
{
    public async Task<DiscordNotificationSettingsEntity?> GetNotificationSettingsForUser(
        int discordNotificationId)
    {
        logger.LogInformation("Fetching Discord notification settings for user with ID {UserId}",
            discordNotificationId);
        try
        {
            var settings = await context.DiscordNotificationSettings
                .FirstOrDefaultAsync(x => x.Id == discordNotificationId);
            if (settings == null)
                logger.LogWarning(
                    "No Discord notification settings found for user with ID {UserId}",
                    discordNotificationId);
            return settings;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "An error occurred while fetching Discord notification settings for user with ID {UserId}",
                discordNotificationId);
            throw;
        }
    }

    public async Task<int> CreateNotificationSettingsForUser(
        DiscordNotificationSettingsEntity entity)
    {
        logger.LogInformation(
            "Creating Discord notification settings for user notification with ID {UserId}",
            entity.UserNotificationId);
        try
        {
            var newSettings = await context.DiscordNotificationSettings.AddAsync(entity);
            await context.SaveChangesAsync();
            logger.LogInformation(
                "Successfully created Discord notification settings with ID {SettingsId} for user notification with ID {UserId}",
                newSettings.Entity.Id, entity.UserNotificationId);
            return newSettings.Entity.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "An error occurred while creating Discord notification settings for user notification with ID {UserId}",
                entity.UserNotificationId);
            throw;
        }
    }

    public async Task<int> UpdateNotificationSettingsForUser(
        DiscordNotificationSettingsEntity entity)
    {
        logger.LogInformation("Updating Discord notification settings for user with ID {UserId}",
            entity.UserNotificationId);
        try
        {
            var existingSettings = await context.DiscordNotificationSettings
                .FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (existingSettings == null)
            {
                logger.LogWarning(
                    "No existing Discord notification settings found for user with ID {UserId}",
                    entity.UserNotificationId);
                throw new NullReferenceException("Settings not found");
            }

            context.Entry(existingSettings).CurrentValues.SetValues(entity);
            await context.SaveChangesAsync();
            logger.LogInformation(
                "Successfully updated Discord notification settings for user with ID {UserId}",
                entity.UserNotificationId);
            return existingSettings.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "An error occurred while updating Discord notification settings for user with ID {UserId}",
                entity.UserNotificationId);
            throw;
        }
    }

    private async Task<DiscordNotificationSettingsEntity?> ValidateUserAccess(int userId,
        int discordNotificationId)
    {
        var settings = await context.DiscordNotificationSettings
            .Include(x => x.UserNotification)
            .FirstOrDefaultAsync(x => x.Id == discordNotificationId);

        if (settings == null || settings.UserNotification.UserId != userId)
            throw new UnauthorizedAccessException(
                "You are not authorized to access these settings.");

        return settings;
    }
}