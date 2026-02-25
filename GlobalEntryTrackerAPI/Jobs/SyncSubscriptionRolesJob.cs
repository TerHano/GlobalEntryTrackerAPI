using Business;
using Microsoft.Extensions.Logging;
using Quartz;

namespace GlobalEntryTrackerAPI.Jobs;

public class SyncSubscriptionRolesJob(
    SubscriptionBusiness subscriptionBusiness,
    ILogger<SyncSubscriptionRolesJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var result = await subscriptionBusiness.SyncAllUserSubscriptionRoles();
        logger.LogInformation(
            "Subscription role sync completed. Total={TotalUsers}, Active={ActiveSubscriptions}, Free={FreeUsers}",
            result.TotalUsers, result.ActiveSubscriptions, result.FreeUsers);
    }
}

