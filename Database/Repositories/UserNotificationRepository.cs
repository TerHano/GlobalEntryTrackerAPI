using Database.Entities;
using Database.Entities.NotificationSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class UserNotificationRepository(
    GlobalEntryTrackerDbContext context,
    ILogger<UserNotificationRepository> logger)
{
    public async Task<UserNotificationEntity> GetUserWithNotificationSettings(int userId)
    {
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

    //Create a new user notification
    public async Task<int> CreateUserNotification(int userId)
    {
        var newUserNotification = await context.UserNotifications.AddAsync(
            new UserNotificationEntity
            {
                UserId = userId
            });
        await context.SaveChangesAsync();
        return newUserNotification.Entity.Id;
    }

    //Update a user notification
    public async Task<int> UpdateUserDiscordNotificationSettings(int userId,
        DiscordNotificationSettingsEntity discordNotificationSettingsEntity)
    {
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

        discordNotificationSettingsEntity.UserNotificationId = userNotification.Id;
        userNotification.DiscordNotificationSettings = discordNotificationSettingsEntity;
        //context.UserNotifications.Update(entity);
        await context.SaveChangesAsync();
        return userNotification.Id;
    }

    public async Task<int> UpdateUserEmailNotificationSettings(int userId,
        EmailNotificationSettingsEntity emailNotificationSettingsEntity)
    {
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

        emailNotificationSettingsEntity.UserNotificationId = userNotification.Id;
        userNotification.EmailNotificationSettings = emailNotificationSettingsEntity;

        await context.SaveChangesAsync();
        return emailNotificationSettingsEntity.Id;
    }
}