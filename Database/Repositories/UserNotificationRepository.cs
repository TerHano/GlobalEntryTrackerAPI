using Database.Entities;
using Database.Entities.NotificationSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class UserNotificationRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    ILogger<UserNotificationRepository> logger)
{
    public async Task<UserNotificationEntity> GetById(int notificationId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var notification = await context.UserNotifications.FindAsync(notificationId);
        if (notification is not null) return notification;
        logger.LogError("UserNotification does not exist");
        throw new NullReferenceException("UserNotification does not exist");
    }

    public async Task<UserNotificationEntity> GetUserWithNotificationSettings(string userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var userNotification = await context.UserNotifications
            .Include(x => x.User)
            .Include(x => x.DiscordNotificationSettings)
            .Include(x => x.EmailNotificationSettings)
            .FirstOrDefaultAsync(user => user.UserId == userId);
        if (userNotification is null)
        {
            logger.LogError("UserNotification does not exist");
            throw new NullReferenceException("UserNotification does not exist");
        }

        return userNotification;
    }

    public async Task<int> CreateUserNotification(string userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var newUserNotification = await context.UserNotifications.AddAsync(
            new UserNotificationEntity
            {
                UserId = userId
            });
        await context.SaveChangesAsync();
        return newUserNotification.Entity.Id;
    }

    public async Task<int> UpdateUserDiscordNotificationSettings(string userId,
        DiscordNotificationSettingsEntity discordNotificationSettingsEntity)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var userNotification = await context.UserNotifications
            .FirstOrDefaultAsync(x => x.UserId == userId);
        if (userNotification is null)
        {
            logger.LogError("UserNotification does not exist");
            throw new NullReferenceException("UserNotification does not exist");
        }

        if (userNotification.DiscordNotificationSettingsId.HasValue &&
            userNotification.DiscordNotificationSettingsId != discordNotificationSettingsEntity.Id)
            throw new UnauthorizedAccessException("No access to this notification settings");
        userNotification.DiscordNotificationSettings = discordNotificationSettingsEntity;
        await context.SaveChangesAsync();
        return userNotification.Id;
    }

    public async Task<int> UpdateUserEmailNotificationSettings(string userId,
        EmailNotificationSettingsEntity emailNotificationSettingsEntity)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var userNotification = await context.UserNotifications
            .FirstOrDefaultAsync(x => x.UserId == userId);
        if (userNotification is null)
        {
            logger.LogError("UserNotification does not exist");
            throw new NullReferenceException("UserNotification does not exist");
        }

        if (userNotification.EmailNotificationSettingsId.HasValue &&
            userNotification.EmailNotificationSettingsId != emailNotificationSettingsEntity.Id)
            throw new UnauthorizedAccessException("No access to this notification settings");
        userNotification.EmailNotificationSettings = emailNotificationSettingsEntity;
        await context.SaveChangesAsync();
        return emailNotificationSettingsEntity.Id;
    }
}