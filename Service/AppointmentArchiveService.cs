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
        foreach (var appointment in appointments)
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
                StartTimestamp = appointment.StartTimestamp.ToUniversalTime(),
                EndTimestamp = appointment.EndTimestamp.ToUniversalTime(),
                ScannedAt = DateTime.UtcNow
            };
            archivedAppointments.Add(archivedAppointment);
        }

        await archivedAppointmentsRepository.AddArchivedAppointments(archivedAppointments);
    }
}