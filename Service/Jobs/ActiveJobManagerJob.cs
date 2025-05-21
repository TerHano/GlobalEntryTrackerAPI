using Database.Repositories;
using Quartz;

namespace Service.Jobs;

public class ActiveJobManagerJob(
    TrackedLocationForUserRepository trackedLocationForUserRepository,
    JobService jobService)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var getAllDistinctActiveIds =
            await trackedLocationForUserRepository.GetAllActiveDistinctLocationTrackerLocationIds();
        foreach (var id in getAllDistinctActiveIds)
            await jobService.StartTrackingAppointmentLocation(id);
    }
}