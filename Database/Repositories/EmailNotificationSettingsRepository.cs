using Database.Entities.NotificationSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class EmailNotificationSettingsRepository(
    GlobalEntryTrackerDbContext context,
    ILogger<EmailNotificationSettingsRepository> logger)
{
    public async Task<EmailNotificationSettingsEntity?> GetNotificationSettingsForUser(
        int emailNotificationId, int userId)
    {
        logger.LogInformation(
            "Fetching Email notification settings for user with ID {UserNotificationId}",
            emailNotificationId);
        try
        {
            var settings = await ValidateUserAccess(userId, emailNotificationId);
            if (settings == null)
                logger.LogWarning(
                    "No Email notification settings found for user with ID {UserNotificationId}",
                    emailNotificationId);
            return settings;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "An error occurred while fetching Email notification settings for user with ID {UserNotificationId}",
                emailNotificationId);
            throw;
        }
    }

    public async Task<int> CreateNotificationSettingsForUser(
        EmailNotificationSettingsEntity entity)
    {
        logger.LogInformation(
            "Creating Email notification settings for user with ID {UserNotificationId}",
            entity.UserNotificationId);
        try
        {
            var newSettings = await context.EmailNotificationSettings.AddAsync(entity);
            await context.SaveChangesAsync();
            logger.LogInformation(
                "Successfully created Email notification settings with ID {SettingsId} for user with ID {UserNotificationId}",
                newSettings.Entity.Id, entity.UserNotificationId);
            return newSettings.Entity.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "An error occurred while creating Email notification settings for user with ID {UserNotificationId}",
                entity.UserNotificationId);
            throw;
        }
    }

    public async Task<int> UpdateNotificationSettingsForUser(
        EmailNotificationSettingsEntity entity, int userId)
    {
        logger.LogInformation(
            "Updating Email notification settings for user with ID {UserNotificationId}",
            entity.UserNotificationId);
        try
        {
            var existingSettings = await ValidateUserAccess(userId, entity.Id);
            if (existingSettings == null)
            {
                logger.LogWarning(
                    "No existing Email notification settings found for user with ID {UserNotificationId}",
                    entity.UserNotificationId);
                throw new NullReferenceException("Settings not found");
            }

            context.Entry(existingSettings).CurrentValues.SetValues(entity);
            await context.SaveChangesAsync();
            logger.LogInformation(
                "Successfully updated Email notification settings for user with ID {UserNotificationId}",
                entity.UserNotificationId);
            return existingSettings.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "An error occurred while updating Email notification settings for user with ID {UserNotificationId}",
                entity.UserNotificationId);
            throw;
        }
    }

    private async Task<EmailNotificationSettingsEntity?> ValidateUserAccess(int userId,
        int emailNotificationId)
    {
        var settings = await context.EmailNotificationSettings
            .Include(x => x.UserNotification)
            .FirstOrDefaultAsync(x => x.Id == emailNotificationId);

        if (settings == null || settings.UserNotification.UserId != userId)
            throw new UnauthorizedAccessException(
                "You are not authorized to access these settings.");

        return settings;
    }
}