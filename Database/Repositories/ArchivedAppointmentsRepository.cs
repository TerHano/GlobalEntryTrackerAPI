using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class ArchivedAppointmentsRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory)
{
    public async Task<List<ArchivedAppointmentEntity>> GetAllArchivedAppointments()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.ArchivedAppointments.ToListAsync();
    }

    public async Task<ArchivedAppointmentEntity?> GetArchivedAppointmentByLocationId(int id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.ArchivedAppointments.FindAsync(id);
    }

    public async Task AddArchivedAppointments(List<ArchivedAppointmentEntity> appointments)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await context.ArchivedAppointments.AddRangeAsync(appointments);
        await context.SaveChangesAsync();
    }

    // public async Task DeleteArchivedAppointment(int id)
    // {
    //     await using var context = await contextFactory.CreateDbContextAsync();
    //     var appointment = await GetArchivedAppointmentById(id);
    //     if (appointment != null)
    //     {
    //         context.ArchivedAppointments.Remove(appointment);
    //         await context.SaveChangesAsync();
    //     }
    // }
}