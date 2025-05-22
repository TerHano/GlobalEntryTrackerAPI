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
    public async Task ArchiveAppointments(List<LocationAppointmentDto> appointments)
    {
        var allAppointments = await appointmentLocationRepository.GetAllAppointmentLocations();
        var archivedAppointments = new List<ArchivedAppointmentEntity>();
        //Get only appointments with distinct dates
        var scanTime = DateTime.UtcNow;
        //
        //This is to avoid duplicates

        var distinctDates = appointments
            .GroupBy(x => DateOnly.FromDateTime(x.StartTimestamp))
            .Select(group => group.First())
            .ToList();

        foreach (var appointment in distinctDates)
        {
            var locationDetails =
                allAppointments.FirstOrDefault(x => x.ExternalId == appointment.ExternalLocationId);
            if (locationDetails == null)
            {
                logger.LogWarning("Location details not found for external ID: {ExternalId}",
                    appointment.ExternalLocationId);
                continue;
            }

            var archivedAppointment = new ArchivedAppointmentEntity
            {
                LocationId = locationDetails.Id,
                Date = DateOnly.FromDateTime(appointment.StartTimestamp),
                ScannedAt = scanTime
            };
            archivedAppointments.Add(archivedAppointment);
        }

        await archivedAppointmentsRepository.AddArchivedAppointments(archivedAppointments);
    }
}