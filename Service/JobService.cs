using Database.Repositories;
using Microsoft.Extensions.Configuration;
using Quartz;
using Service.Jobs;

namespace Service;

public class JobService(
    ISchedulerFactory schedulerFactory,
    AppointmentLocationRepository appointmentLocationRepository,
    IConfiguration configuration)

{
    public async Task StartTrackingAppointmentLocation(int locationId)
    {
        var scheduler = await schedulerFactory.GetScheduler();
        var jobIntervalStr = configuration["Jobs:Location_Fetch_Interval_In_Minutes"];
        if (string.IsNullOrEmpty(jobIntervalStr) ||
            !int.TryParse(jobIntervalStr,
                out var jobInterval)) jobInterval = 10; // Default to 10 minutes if not configured

        var alreadyRunning = await scheduler.CheckExists(GetJobKey(locationId));
        if (!alreadyRunning)
        {
            // Get the appointment location
            var appointmentLocation =
                await appointmentLocationRepository.GetAppointmentLocationById(locationId);
            // Define the job
            var job = JobBuilder.Create<GetLatestAppointmentsForLocationJob>()
                .WithIdentity(GetJobKey(locationId))
                .UsingJobData("externalLocationId", appointmentLocation.ExternalId)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity(GetTriggerKey(locationId))
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(jobInterval)
                    .RepeatForever())
                .Build();

            // Schedule the job with the trigger
            await scheduler.ScheduleJob(job, trigger);
        }
    }

    private static JobKey GetJobKey(int locationId)
    {
        return new JobKey($"Job-{locationId}", "LocationTrackers");
    }

    private static TriggerKey GetTriggerKey(int locationId)
    {
        return new TriggerKey("EveryConfiguredMin", $"Trigger-{locationId}");
    }
}