using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class TrackedLocationForUserRepository(
    GlobalEntryTrackerDbContext context,
    ILogger<TrackedLocationForUserRepository> logger)
{
    public async Task<TrackedLocationForUserEntity> GetTrackerById(int trackerId)
    {
        var trackedLocation = await context.UserTrackedLocations
            .Include(x => x.Location)
            .Include(x => x.NotificationType)
            .FirstOrDefaultAsync(x => x.Id == trackerId);
        if (trackedLocation == null) throw new NullReferenceException("Tracker not found");
        return trackedLocation;
    }

    public List<TrackedLocationForUserEntity> GetTrackersByLocationId(int locationId)
    {
        return context.UserTrackedLocations.Include(x => x.NotificationType)
            .Include(x => x.Location).Where(x => x.LocationId == locationId).ToList();
    }

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

    public async Task<List<TrackedLocationForUserEntity>> GetTrackedLocationsForUser(int userId)
    {
        return await context.UserTrackedLocations.Include(x => x.NotificationType)
            .Include(x => x.Location).Where(x => x.UserId == userId).ToListAsync();
    }

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

    public async Task<List<int>> GetAllActiveDistinctLocationTrackerLocationIds()
    {
        var externalLocationIds = await context.UserTrackedLocations
            // .Include(x => x.Location)
            .Where(x => x.Enabled == true)
            .Select(x => x.LocationId)
            .Distinct()
            .ToListAsync();
        return externalLocationIds;
    }

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
}