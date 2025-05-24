using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.Logging;
using Service.Dto;

namespace Service;

public class AppointmentArchiveService(
    ArchivedAppointmentsRepository archivedAppointmentsRepository,
    AppointmentLocationRepository appointmentLocationRepository,
    ILogger<AppointmentArchiveService> logger)
{
    public async Task ArchiveAppointments(List<LocationAppointmentDto> appointments,
        DateTime scanTime)
    {
        var allAppointments = await appointmentLocationRepository.GetAllAppointmentLocations();
        var archivedAppointments = new List<ArchivedAppointmentEntity>();


        var distinctDates = appointments
            .GroupBy(x => DateOnly.FromDateTime(x.StartTimestamp))
            //.Select(group => group.First())
            .ToList();

        foreach (var appointmentsForDate in distinctDates)
        {
            var externalLocationId = appointmentsForDate.FirstOrDefault()?.ExternalLocationId;
            if (externalLocationId == null)
            {
                logger.LogWarning("External location ID is null for appointment on {Date}",
                    appointmentsForDate.FirstOrDefault()?.StartTimestamp);
                continue;
            }

            var locationDetails =
                allAppointments.FirstOrDefault(x => x.ExternalId == externalLocationId);
            if (locationDetails == null)
            {
                logger.LogWarning("Location details not found for external ID: {ExternalId}",
                    externalLocationId);
                continue;
            }

            var archivedAppointment = new ArchivedAppointmentEntity
            {
                LocationId = locationDetails.Id,
                Date = appointmentsForDate.Key,
                NumberOfAppointments = appointmentsForDate.Count(),
                ScannedAt = scanTime
            };
            archivedAppointments.Add(archivedAppointment);
        }

        await archivedAppointmentsRepository.AddArchivedAppointments(archivedAppointments);
    }
}