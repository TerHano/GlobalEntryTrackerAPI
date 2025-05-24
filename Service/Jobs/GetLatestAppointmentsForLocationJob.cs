using System.Text.Json;
using Quartz;
using Service.Dto;

namespace Service.Jobs;

public class GetLatestAppointmentsForLocationJob(
    HttpClient httpClient,
    NotificationDispatcherService notificationDispatcherService,
    AppointmentArchiveService appointmentArchiveService) : IJob
{
    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Executing job to get latest appointments for location");
        var externalLocationId = context.JobDetail.JobDataMap.GetIntValue("externalLocationId");
        var appointmentUrl =
            $"https://ttp.cbp.dhs.gov/schedulerapi/slots?orderBy=soonest&locationId={externalLocationId}&minimum=1&limit=100";
        var response = await httpClient.GetAsync(appointmentUrl);
        if (response.IsSuccessStatusCode)
        {
            var jsonContent = await response.Content.ReadAsStringAsync();
            var locationAppointments =
                JsonSerializer.Deserialize<List<LocationAppointmentDto>>(jsonContent,
                    _jsonSerializerOptions);
            if (locationAppointments == null || locationAppointments.Count == 0) return;

            var scanTime = DateTime.UtcNow;
            var archiveTask =
                appointmentArchiveService.ArchiveAppointments(locationAppointments, scanTime);
            var notificationTask = notificationDispatcherService.SendNotificationForLocation(
                locationAppointments,
                externalLocationId);
            await Task.WhenAll(archiveTask, notificationTask);
        }
    }
}