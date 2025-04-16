using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class TrackedLocationForUserRepository(GlobalEntryTrackerDbContext context)
{
    public async Task<TrackedLocationForUserEntity> GetTrackerById(int trackerId, int userId)
    {
        var trackedLocation = await context.UserTrackedLocations
            .Include(x => x.Location)
            .Include(x => x.NotificationType)
            .FirstOrDefaultAsync(x => x.Id == trackerId);
        if (trackedLocation == null) throw new NullReferenceException("Tracker not found");
        if (trackedLocation.UserId != userId)
            throw new UnauthorizedAccessException(
                "You are not authorized to access this tracked location.");
        return trackedLocation;
    }

    public List<TrackedLocationForUserEntity> GetTrackersByLocationId(int locationId)
    {
        return context.UserTrackedLocations.Include(x => x.NotificationType)
            .Include(x => x.Location).Where(x => x.LocationId == locationId).ToList();
    }

    public async Task<List<TrackedLocationForUserEntity>> GetTrackedLocationsForUser(int userId)
    {
        return await context.UserTrackedLocations.Where(x => x.UserId == userId).ToListAsync();
    }

    public async Task<int> CreateTrackerForUser(TrackedLocationForUserEntity trackedLocationForUser)
    {
        var newTracker = await context.UserTrackedLocations.AddAsync(trackedLocationForUser);
        await context.SaveChangesAsync();
        return newTracker.Entity.Id;
    }

    public async Task<int> UpdateTrackerForUser(TrackedLocationForUserEntity trackedLocationForUser)
    {
        var entity = await context.UserTrackedLocations.FindAsync(trackedLocationForUser.Id);
        if (entity == null) throw new NullReferenceException("Tracker not found");
        if (entity.UserId != trackedLocationForUser.UserId)
            throw new UnauthorizedAccessException(
                "You are not authorized to update this tracked location.");
        var updatedEntity = context.Update(trackedLocationForUser);
        await context.SaveChangesAsync();
        return updatedEntity.Entity.Id;
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