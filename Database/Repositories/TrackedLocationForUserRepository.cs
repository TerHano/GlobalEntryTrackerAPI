using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

/// <summary>
///     Repository for managing tracked locations for users.
///     Provides methods to perform CRUD operations and retrieve data related to user-tracked locations.
/// </summary>
public class TrackedLocationForUserRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    ILogger<TrackedLocationForUserRepository> logger)
{
    /// <summary>
    ///     Retrieves a tracked location by its ID.
    /// </summary>
    public async Task<TrackedLocationForUserEntity> GetTrackerById(int trackerId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var trackedLocation = await context.UserTrackedLocations
            .Include(x => x.Location)
            .Include(x => x.NotificationType)
            .FirstOrDefaultAsync(x => x.Id == trackerId);
        if (trackedLocation == null) throw new NullReferenceException("Tracker not found");
        return trackedLocation;
    }

    /// <summary>
    ///     Retrieves all trackers associated with a specific location ID.
    /// </summary>
    public List<TrackedLocationForUserEntity> GetTrackersByLocationId(int locationId)
    {
        using var context = contextFactory.CreateDbContext();
        return context.UserTrackedLocations.Include(x => x.NotificationType)
            .Include(x => x.Location).Where(x => x.LocationId == locationId).ToList();
    }

    /// <summary>
    ///     Retrieves trackers for a specific location ID that are due for notification.
    /// </summary>
    public async Task<List<TrackedLocationForUserEntity>> GetTrackersByLocationIdDueForNotification(
        int locationId)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        var now = DateTime.UtcNow;
        return await context.UserTrackedLocations.Include(x => x.NotificationType)
            .Include(x => x.Location)
            .Include(x => x.User)
            .ThenInclude(x => x.UserRole)
            .ThenInclude(x => x.Role)
            .Where(x => x.Enabled && x.LocationId == locationId && x.User.NextNotificationAt < now)
            .ToListAsync();
    }

    /// <summary>
    ///     Retrieves all tracked locations for a specific user.
    /// </summary>
    public async Task<List<TrackedLocationForUserEntity>> GetTrackedLocationsForUser(int userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.UserTrackedLocations.Include(x => x.NotificationType)
            .Include(x => x.Location).Where(x => x.UserId == userId).ToListAsync();
    }

    /// <summary>
    ///     Creates a new tracker for a user.
    /// </summary>
    public async Task<int> CreateTrackerForUser(TrackedLocationForUserEntity trackedLocationForUser)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var newTracker = await context.UserTrackedLocations.AddAsync(trackedLocationForUser);
            await context.SaveChangesAsync();
            return newTracker.Entity.Id;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex.Message);
            throw new DbUpdateException("Failed to create tracker for user", ex);
        }
    }

    /// <summary>
    ///     Updates an existing tracker for a user.
    /// </summary>
    public async Task<int> UpdateTrackerForUser(TrackedLocationForUserEntity trackedLocationForUser)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var entity = context.UserTrackedLocations.Update(trackedLocationForUser);
            await context.SaveChangesAsync();
            return entity.Entity.Id;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex.Message);
            throw new DbUpdateException("Failed to update tracker for user", ex);
        }
    }

    /// <summary>
    ///     Updates a list of tracked locations.
    /// </summary>
    public async Task UpdateTrackers(List<TrackedLocationForUserEntity> trackedLocations)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            context.UserTrackedLocations.UpdateRange(trackedLocations);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex.Message);
            throw new DbUpdateException("Failed to update tracker for user", ex);
        }
    }

    /// <summary>
    ///     Retrieves all distinct location IDs for active trackers.
    /// </summary>
    public async Task<List<int>> GetAllActiveDistinctLocationTrackerLocationIds()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var externalLocationIds = await context.UserTrackedLocations
            .Where(x => x.Enabled == true)
            .Select(x => x.LocationId)
            .Distinct()
            .ToListAsync();
        return externalLocationIds;
    }

    /// <summary>
    ///     Deletes a tracker for a user by tracker ID.
    /// </summary>
    public async Task<int> DeleteTrackerForUser(int trackerId, int userId)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var entityToDelete = await context.UserTrackedLocations.FindAsync(trackerId);
            if (entityToDelete == null) throw new NullReferenceException("Tracker not found");

            if (entityToDelete.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not authorized to delete this tracked location.");
            var removedEntity = context.UserTrackedLocations.Remove(entityToDelete);
            await context.SaveChangesAsync();
            return removedEntity.Entity.Id;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex.Message);
            throw new DbUpdateException("Failed to delete tracker for user", ex);
        }
    }

    /// <summary>
    ///     Deletes all trackers for a specific user.
    /// </summary>
    public async Task DeleteAllTrackersForUser(int userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var trackedLocations = await context.UserTrackedLocations
            .Where(x => x.UserId == userId)
            .ToListAsync();
        context.UserTrackedLocations.RemoveRange(trackedLocations);
        await context.SaveChangesAsync();
    }
}