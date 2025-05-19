using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class AppointmentLocationRepository(
    GlobalEntryTrackerDbContext context,
    ILogger<GlobalEntryTrackerDbContext> logger)
{
    public async Task<List<AppointmentLocationEntity>> GetAllAppointmentLocations()
    {
        logger.LogInformation("Fetching all appointment locations.");
        return await context.AppointmentLocations.ToListAsync();
    }

    public async Task<List<string>> GetAppointmentStates()
    {
        logger.LogInformation("Fetching distinct appointment states.");
        return await context.AppointmentLocations
            .Select(x => x.State)
            .Distinct()
            .ToListAsync();
    }

    public async Task<AppointmentLocationEntity> GetAppointmentLocationByExternalId(int id)
    {
        logger.LogInformation("Fetching appointment location with external ID {ExternalId}.", id);
        var appointmentLocation = await context.AppointmentLocations
            .FirstOrDefaultAsync(x => x.ExternalId == id);
        if (appointmentLocation == null)
        {
            logger.LogWarning("Appointment location with external ID {ExternalId} not found.", id);
            throw new NullReferenceException("Appointment location not found");
        }

        return appointmentLocation;
    }

    public async Task<AppointmentLocationEntity> GetAppointmentLocationById(int id)
    {
        logger.LogInformation("Fetching appointment location with ID {Id}.", id);
        var appointmentLocation = await context.AppointmentLocations
            .FirstOrDefaultAsync(x => x.Id == id);
        if (appointmentLocation == null)
        {
            logger.LogWarning("Appointment location with ID {Id} not found.", id);
            throw new NullReferenceException("Appointment location not found");
        }

        return appointmentLocation;
    }

    public async Task CreateAppointmentLocation(AppointmentLocationEntity appointmentLocation)
    {
        logger.LogInformation("Creating a new appointment location.");
        try
        {
            await context.AppointmentLocations.AddAsync(appointmentLocation);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to create appointment location.");
            throw new DbUpdateException("Failed to create appointment location", ex);
        }
    }

    public async Task UpdateAppointmentLocation(AppointmentLocationEntity appointmentLocation)
    {
        logger.LogInformation("Updating appointment location with ID {Id}.",
            appointmentLocation.Id);
        try
        {
            context.AppointmentLocations.Update(appointmentLocation);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to update appointment location with ID {Id}.",
                appointmentLocation.Id);
            throw new DbUpdateException("Failed to update appointment location", ex);
        }
    }
}