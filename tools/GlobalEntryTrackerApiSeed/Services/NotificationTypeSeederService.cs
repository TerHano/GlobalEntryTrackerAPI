using Database;
using Database.Entities;
using GlobalEntryTrackerAPI;
using GlobalEntryTrackerApiSeed.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GlobalEntryTrackerApiSeed.Services;

public class NotificationTypeSeederService(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    ILogger<NotificationTypeSeederService> logger)
{
    public async Task<NotificationTypeSeedResult> SeedNotificationTypesAsync(bool dryRun)
    {
        logger.LogInformation("Starting notification type seeding (dry-run={DryRun})", dryRun);

        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            var existingNotificationTypes = await context.NotificationTypes.ToListAsync();
            logger.LogInformation("Found {Count} existing notification types",
                existingNotificationTypes.Count);

            var notificationTypesToSeed = GetNotificationTypes();
            var addedCount = 0;
            var skippedCount = 0;
            var addedTypes = new List<string>();

            foreach (var notificationType in notificationTypesToSeed)
            {
                var exists = existingNotificationTypes.Any(nt =>
                    nt.Type == notificationType.Type);

                if (exists)
                {
                    logger.LogInformation(
                        "Notification type '{Name}' (Type={Type}) already exists, skipping",
                        notificationType.Name, notificationType.Type);
                    skippedCount++;
                }
                else
                {
                    if (!dryRun) context.NotificationTypes.Add(notificationType);

                    logger.LogInformation(
                        "Adding notification type '{Name}' (Type={Type})",
                        notificationType.Name, notificationType.Type);
                    addedCount++;
                    addedTypes.Add(notificationType.Name);
                }
            }

            if (!dryRun && addedCount > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Saved {Count} notification types to database", addedCount);
            }

            var result = new NotificationTypeSeedResult
            {
                Success = true,
                Added = addedCount,
                Skipped = skippedCount,
                AddedTypes = addedTypes
            };

            logger.LogInformation(
                "Notification type seeding completed: Added={Added}, Skipped={Skipped}",
                addedCount, skippedCount);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding notification types");
            return new NotificationTypeSeedResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static List<NotificationTypeEntity> GetNotificationTypes()
    {
        var weekendNotificationType = new NotificationTypeEntity
        {
            Name = "Weekend",
            Description = "Notify about weekend entries",
            Type = NotificationType.Weekends
        };

        var soonestNotificationType = new NotificationTypeEntity
        {
            Name = "Soonest",
            Description = "Notify about soonest entries",
            Type = NotificationType.Soonest
        };

        return
        [
            weekendNotificationType,
            soonestNotificationType
        ];
    }
}