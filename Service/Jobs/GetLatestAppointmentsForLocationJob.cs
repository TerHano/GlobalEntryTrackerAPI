using System.Text.Json;
using Quartz;
using Service.Dto;

namespace Service.Jobs;

public class GetLatestAppointmentsForLocationJob(
    HttpClient httpClient,
    NotificationDispatcherService notificationDispatcherService) : IJob
{
    private readonly JsonSerializerOptions jsonSerializerOptions =
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
            $"https://ttp.cbp.dhs.gov/schedulerapi/slots?orderBy=soonest&locationId={externalLocationId}&minimum=1&limit=5";
        var response = await httpClient.GetAsync(appointmentUrl);
        if (response.IsSuccessStatusCode)
        {
            var jsonContent = await response.Content.ReadAsStringAsync();
            var locationAppointment =
                JsonSerializer.Deserialize<List<LocationAppointmentDto>>(jsonContent,
                    jsonSerializerOptions);
            if (locationAppointment == null || !locationAppointment.Any()) return;
            await notificationDispatcherService.SendNotificationForLocation(locationAppointment,
                externalLocationId);
        }
    }
}