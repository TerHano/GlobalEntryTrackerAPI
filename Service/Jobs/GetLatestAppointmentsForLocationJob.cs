using System.Text.Json;
using Microsoft.Extensions.Logging;
using Quartz;
using Service.Dto;

namespace Service.Jobs;

// Overlapping scans of the same location would race on recording first-seen times.
[DisallowConcurrentExecution]
public class GetLatestAppointmentsForLocationJob(
    HttpClient httpClient,
    NotificationDispatcherService notificationDispatcherService,
    AppointmentArchiveService appointmentArchiveService,
    ILogger<GetLatestAppointmentsForLocationJob> logger) : IJob
{
    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

    public async Task Execute(IJobExecutionContext context)
    {
        var externalLocationId = context.JobDetail.JobDataMap.GetIntValue("externalLocationId");
        var cancellationToken = context.CancellationToken;
        var appointmentUrl =
            $"https://ttp.cbp.dhs.gov/schedulerapi/slots?orderBy=soonest&locationId={externalLocationId}&minimum=1&limit=100";

        logger.LogDebug("Fetching appointments for external location {ExternalLocationId}",
            externalLocationId);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(appointmentUrl, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex,
                "Appointment fetch failed for external location {ExternalLocationId}",
                externalLocationId);
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            // The CBP scheduler rate-limits; surfacing this is the only way to notice we have gone
            // quiet, since a failed scan is otherwise indistinguishable from "no appointments".
            logger.LogWarning(
                "Appointment fetch for external location {ExternalLocationId} returned {StatusCode}",
                externalLocationId, response.StatusCode);
            return;
        }

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        List<LocationAppointmentDto>? locationAppointments;
        try
        {
            locationAppointments =
                JsonSerializer.Deserialize<List<LocationAppointmentDto>>(jsonContent,
                    _jsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex,
                "Could not parse appointment response for external location {ExternalLocationId}",
                externalLocationId);
            return;
        }

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
