using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

/// <summary>
///     Tracks when each appointment slot was first observed, which is the basis for the per-role
///     notification delay.
/// </summary>
public class SeenAppointmentRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    ILogger<SeenAppointmentRepository> logger)
{
    /// <summary>
    ///     Records any slots not seen before and returns first-seen times for every supplied slot.
    /// </summary>
    /// <param name="locationId">Internal appointment location ID.</param>
    /// <param name="startTimestamps">Slot start times from the current scan.</param>
    /// <param name="scanTime">Timestamp of the current scan, used as first-seen for new slots.</param>
    /// <returns>Map of normalised slot start time to the time it was first observed.</returns>
    public async Task<Dictionary<DateTime, DateTime>> RecordAndGetFirstSeen(int locationId,
        IEnumerable<DateTime> startTimestamps, DateTime scanTime)
    {
        var normalizedStarts = startTimestamps
            .Select(SeenAppointmentEntity.NormalizeStart)
            .Distinct()
            .ToList();
        if (normalizedStarts.Count == 0) return new Dictionary<DateTime, DateTime>();

        await using var context = await contextFactory.CreateDbContextAsync();
        var existing = await context.SeenAppointments
            .AsNoTracking()
            .Where(x => x.LocationId == locationId && normalizedStarts.Contains(x.StartTimestamp))
            .ToListAsync();

        var firstSeenByStart = existing.ToDictionary(x => x.StartTimestamp, x => x.FirstSeenAt);

        var newSlots = normalizedStarts
            .Where(x => !firstSeenByStart.ContainsKey(x))
            .Select(x => new SeenAppointmentEntity
            {
                LocationId = locationId,
                StartTimestamp = x,
                FirstSeenAt = scanTime
            })
            .ToList();

        if (newSlots.Count > 0)
        {
            await context.SeenAppointments.AddRangeAsync(newSlots);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // A concurrent scan of the same location can insert the same slot first. The unique
                // index makes that safe to ignore; re-read so the caller still gets real first-seen
                // times rather than this scan's timestamp.
                logger.LogWarning(ex,
                    "Conflict recording seen appointments for location {LocationId}, re-reading",
                    locationId);
                return await GetFirstSeen(locationId, normalizedStarts);
            }

            foreach (var slot in newSlots)
                firstSeenByStart[slot.StartTimestamp] = slot.FirstSeenAt;
        }

        return firstSeenByStart;
    }

    private async Task<Dictionary<DateTime, DateTime>> GetFirstSeen(int locationId,
        List<DateTime> normalizedStarts)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.SeenAppointments
            .AsNoTracking()
            .Where(x => x.LocationId == locationId && normalizedStarts.Contains(x.StartTimestamp))
            .ToDictionaryAsync(x => x.StartTimestamp, x => x.FirstSeenAt);
    }

    /// <summary>
    ///     Removes records for slots whose start time has passed; they can no longer be notified on.
    /// </summary>
    public async Task<int> PruneSeenAppointmentsBefore(DateTime cutoff)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.SeenAppointments
            .Where(x => x.StartTimestamp < cutoff)
            .ExecuteDeleteAsync();
    }
}
