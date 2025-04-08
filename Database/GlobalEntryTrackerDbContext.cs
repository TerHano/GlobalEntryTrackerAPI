using Database.Entities;
using GlobalEntryTrackerAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class GlobalEntryTrackerDbContext(DbContextOptions<GlobalEntryTrackerDbContext> options): Microsoft.EntityFrameworkCore.DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    {
 
    }

    public DbSet<AppointmentLocationEntity> AppointmentLocations { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<TrackedLocationForUserEntity> UserTrackedLocations { get; set; }

}