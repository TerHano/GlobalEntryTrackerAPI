using System.Text.Json;
using Quartz;
using Service.Dto;

namespace Service.Jobs;

public class GetLatestAppointmentsForLocationJob(
    HttpClient httpClient,
    NotificationDispatcherService notificationDispatcherService) : IJob
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
            $"https://ttp.cbp.dhs.gov/schedulerapi/slots?orderBy=soonest&locationId={externalLocationId}&minimum=1&limit=500";
        var response = await httpClient.GetAsync(appointmentUrl);
        if (response.IsSuccessStatusCode)
        {
            var jsonContent = await response.Content.ReadAsStringAsync();
            var locationAppointment =
                JsonSerializer.Deserialize<List<LocationAppointmentDto>>(jsonContent,
                    _jsonSerializerOptions);
            if (locationAppointment == null || locationAppointment.Count == 0) return;
            await notificationDispatcherService.SendNotificationForLocation(locationAppointment,
                externalLocationId);
        }
    }
}