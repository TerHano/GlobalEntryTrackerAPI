using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class AppointmentLocationRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    ILogger<AppointmentLocationRepository> logger,
    IMemoryCache memoryCache)
{
    public async Task<List<AppointmentLocationEntity>> GetAllAppointmentLocations()
    {
        logger.LogInformation("Fetching all appointment locations.");
        return await memoryCache.GetOrCreateAsync("AppointmentLocations", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.AppointmentLocations.AsNoTracking().ToListAsync();
        }) ?? [];
    }

    public async Task<List<string>> GetAppointmentStates()
    {
        logger.LogInformation("Fetching distinct appointment states.");
        return await memoryCache.GetOrCreateAsync("AppointmentLocationStates", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            await using var context = await contextFactory.CreateDbContextAsync();
            return await context.AppointmentLocations
                .Select(x => x.State)
                .Distinct()
                .ToListAsync();
        }) ?? [];
    }

    public async Task<AppointmentLocationEntity> GetAppointmentLocationByExternalId(int id)
    {
        logger.LogInformation("Fetching appointment location with external ID {ExternalId}.", id);
        await using var context = await contextFactory.CreateDbContextAsync();
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
        await using var context = await contextFactory.CreateDbContextAsync();
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
            await using var context = await contextFactory.CreateDbContextAsync();
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
            await using var context = await contextFactory.CreateDbContextAsync();
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