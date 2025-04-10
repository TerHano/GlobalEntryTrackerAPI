using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class AppointmentLocationRepository(GlobalEntryTrackerDbContext context)
{
    public async Task<List<AppointmentLocationEntity>> GetAllAppointmentLocations()
    {
        return await context.AppointmentLocations.ToListAsync();
    }

    public async Task<AppointmentLocationEntity> GetAppointmentLocationByExternalId(int id)
    {
        var appointmentLocation = await context.AppointmentLocations
            .FirstOrDefaultAsync(x => x.ExternalId == id);
        if (appointmentLocation == null)
            throw new NullReferenceException("Appointment location not found");
        return appointmentLocation;
    }

    public async Task CreateAppointmentLocation(AppointmentLocationEntity appointmentLocation)
    {
        await context.AppointmentLocations.AddAsync(appointmentLocation);
        await context.SaveChangesAsync();
    }
}