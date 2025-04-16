using Database.Repositories;
using Quartz;
using Service.Jobs;

namespace Service;

public class JobService(
    ISchedulerFactory schedulerFactory,
    AppointmentLocationRepository appointmentLocationRepository)
{
    public async Task StartTrackingAppointmentLocation(int locationId)
    {
        var scheduler = await schedulerFactory.GetScheduler();
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
                .WithIdentity("5min", "LocationTrackers")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(5)
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
}