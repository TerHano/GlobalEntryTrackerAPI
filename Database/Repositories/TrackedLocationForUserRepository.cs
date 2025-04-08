using Database.Entities;

namespace Database.Repositories;

public class TrackedLocationForUserRepository(GlobalEntryTrackerDbContext context)
{
    public List<TrackedLocationForUserEntity> GetTrackersByLocationId(int locationId)
    {
        return context.UserTrackedLocations.Where(x => x.LocationId == locationId).ToList();
    }

    public List<TrackedLocationForUserEntity> GetTrackedLocationsForUser(int userId)
    {
        return context.UserTrackedLocations.Where(x => x.UserId == userId).ToList();
    }

    public void CreateTracker(TrackedLocationForUserEntity trackedLocationForUser)
    {
        context.UserTrackedLocations.Add(trackedLocationForUser);
        context.SaveChanges();
    }
    
    
}