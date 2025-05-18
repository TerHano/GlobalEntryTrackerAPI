using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class TrackedLocationForUserRepository(GlobalEntryTrackerDbContext context)
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
            .ThenInclude(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Where(x => x.LocationId == locationId && x.NextNotificationAt <= now).ToList();
    }

    public async Task<List<TrackedLocationForUserEntity>> GetTrackedLocationsForUser(int userId)
    {
        return await context.UserTrackedLocations.Include(x => x.NotificationType)
            .Include(x => x.Location).Where(x => x.UserId == userId).ToListAsync();
    }

    public async Task<int> CreateTrackerForUser(TrackedLocationForUserEntity trackedLocationForUser)
    {
        var newTracker = await context.UserTrackedLocations.AddAsync(trackedLocationForUser);
        await context.SaveChangesAsync();
        return newTracker.Entity.Id;
    }

    public async Task<int> UpdateTrackerForUser(TrackedLocationForUserEntity trackedLocationForUser)
    {
        var entity = context.Update(trackedLocationForUser);
        await context.SaveChangesAsync();
        return entity.Entity.Id;
    }

    public async Task<List<int>> UpdateListOfTrackers(
        List<TrackedLocationForUserEntity> trackedLocationsForUser)
    {
        var updatedTrackers = new List<int>();
        foreach (var trackedLocationForUser in trackedLocationsForUser)
        {
            var entity = context.Update(trackedLocationForUser);
            updatedTrackers.Add(entity.Entity.Id);
        }

        await context.SaveChangesAsync();
        return updatedTrackers;
    }

    public async Task<int> DeleteTrackerForUser(int trackerId, int userId)
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
}