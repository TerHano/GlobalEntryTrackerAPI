using Database.Repositories;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Service.Jobs;

public class ActiveJobManagerJob(
    TrackedLocationForUserRepository trackedLocationForUserRepository,
    SeenAppointmentRepository seenAppointmentRepository,
    JobService jobService,
    ILogger<ActiveJobManagerJob> logger)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var getAllDistinctActiveIds =
            await trackedLocationForUserRepository.GetAllActiveDistinctLocationTrackerLocationIds();
        foreach (var id in getAllDistinctActiveIds)
            await jobService.StartTrackingAppointmentLocation(id);

        // Slots that have already started can no longer be notified on, so their first-seen
        // records are dead weight.
        var pruned = await seenAppointmentRepository.PruneSeenAppointmentsBefore(DateTime.UtcNow);
        if (pruned > 0) logger.LogInformation("Pruned {Count} expired seen appointments", pruned);
    }
}
