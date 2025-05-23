using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

/// <summary>
///     Repository for managing tracked locations for users.
///     Provides methods to perform CRUD operations and retrieve data related to user-tracked locations.
/// </summary>
public class TrackedLocationForUserRepository(
    GlobalEntryTrackerDbContext context,
    ILogger<TrackedLocationForUserRepository> logger)
{
    /// <summary>
    ///     Retrieves a tracked location by its ID.
    /// </summary>
    /// <param name="trackerId">The ID of the tracker to retrieve.</param>
    /// <returns>The tracked location entity.</returns>
    /// <exception cref="NullReferenceException">Thrown if the tracker is not found.</exception>
    public async Task<TrackedLocationForUserEntity> GetTrackerById(int trackerId)
    {
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
    /// <param name="locationId">The ID of the location.</param>
    /// <returns>A list of tracked location entities.</returns>
    public List<TrackedLocationForUserEntity> GetTrackersByLocationId(int locationId)
    {
        return context.UserTrackedLocations.Include(x => x.NotificationType)
            .Include(x => x.Location).Where(x => x.LocationId == locationId).ToList();
    }

    /// <summary>
    ///     Retrieves trackers for a specific location ID that are due for notification.
    /// </summary>
    /// <param name="locationId">The ID of the location.</param>
    /// <returns>A list of tracked location entities due for notification.</returns>
    public List<TrackedLocationForUserEntity> GetTrackersByLocationIdDueForNotification(
        int locationId)
    {
        var now = DateTime.UtcNow;
        return context.UserTrackedLocations.Include(x => x.NotificationType)
            .Include(x => x.Location)
            .Include(x => x.User)
            .ThenInclude(x => x.UserRole)
            .ThenInclude(x => x.Role)
            .Where(x => x.LocationId == locationId && x.User.NextNotificationAt < now).ToList();
    }

    /// <summary>
    ///     Retrieves all tracked locations for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of tracked location entities for the user.</returns>
    public async Task<List<TrackedLocationForUserEntity>> GetTrackedLocationsForUser(int userId)
    {
        return await context.UserTrackedLocations.Include(x => x.NotificationType)
            .Include(x => x.Location).Where(x => x.UserId == userId).ToListAsync();
    }

    /// <summary>
    ///     Creates a new tracker for a user.
    /// </summary>
    /// <param name="trackedLocationForUser">The tracked location entity to create.</param>
    /// <returns>The ID of the newly created tracker.</returns>
    /// <exception cref="DbUpdateException">Thrown if the creation fails.</exception>
    public async Task<int> CreateTrackerForUser(TrackedLocationForUserEntity trackedLocationForUser)
    {
        try
        {
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
    /// <param name="trackedLocationForUser">The tracked location entity to update.</param>
    /// <returns>The ID of the updated tracker.</returns>
    /// <exception cref="DbUpdateException">Thrown if the update fails.</exception>
    public async Task<int> UpdateTrackerForUser(TrackedLocationForUserEntity trackedLocationForUser)
    {
        try
        {
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
    /// <param name="trackedLocations">The list of tracked location entities to update.</param>
    /// <exception cref="DbUpdateException">Thrown if the update fails.</exception>
    public async Task UpdateTrackers(List<TrackedLocationForUserEntity> trackedLocations)
    {
        try
        {
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
    /// <returns>A list of distinct location IDs.</returns>
    public async Task<List<int>> GetAllActiveDistinctLocationTrackerLocationIds()
    {
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
    /// <param name="trackerId">The ID of the tracker to delete.</param>
    /// <param name="userId">The ID of the user requesting the deletion.</param>
    /// <returns>The ID of the deleted tracker.</returns>
    /// <exception cref="NullReferenceException">Thrown if the tracker is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user is not authorized to delete the tracker.</exception>
    /// <exception cref="DbUpdateException">Thrown if the deletion fails.</exception>
    public async Task<int> DeleteTrackerForUser(int trackerId, int userId)
    {
        try
        {
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
    /// <param name="userId">The ID of the user whose trackers will be deleted.</param>
    public async Task DeleteAllTrackersForUser(int userId)
    {
        var trackedLocations = await context.UserTrackedLocations
            .Where(x => x.UserId == userId)
            .ToListAsync();
        context.UserTrackedLocations.RemoveRange(trackedLocations);
        await context.SaveChangesAsync();
    }
}