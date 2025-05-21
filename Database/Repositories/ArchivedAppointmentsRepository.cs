using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class ArchivedAppointmentsRepository(GlobalEntryTrackerDbContext context)
{
    public async Task<List<ArchivedAppointmentEntity>> GetAllArchivedAppointments()
    {
        return await context.ArchivedAppointments.ToListAsync();
    }

    public async Task<ArchivedAppointmentEntity?> GetArchivedAppointmentByLocationId(int id)
    {
        return await context.ArchivedAppointments.FindAsync(id);
    }

    public async Task AddArchivedAppointment(ArchivedAppointmentEntity appointment)
    {
        await context.ArchivedAppointments.AddAsync(appointment);
        await context.SaveChangesAsync();
    }

    public async Task AddArchivedAppointments(List<ArchivedAppointmentEntity> appointments)
    {
        await context.ArchivedAppointments.AddRangeAsync(appointments);
        await context.SaveChangesAsync();
    }

    // public async Task DeleteArchivedAppointment(int id)
    // {
    //     var appointment = await GetArchivedAppointmentById(id);
    //     if (appointment != null)
    //     {
    //         context.ArchivedAppointments.Remove(appointment);
    //         await context.SaveChangesAsync();
    //     }
    // }
}