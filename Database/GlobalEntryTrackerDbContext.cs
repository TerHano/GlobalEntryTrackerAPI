using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class GlobalEntryTrackerDbContext(DbContextOptions<GlobalEntryTrackerDbContext> options): Microsoft.EntityFrameworkCore.DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrackedLocationForUserEntity>()
            .HasOne(e => e.Location)
            .WithMany()
            .HasForeignKey(e => e.LocationId);
        
        modelBuilder.Entity<TrackedLocationForUserEntity>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);
    }

    public DbSet<AppointmentLocationEntity> AppointmentLocations { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<TrackedLocationForUserEntity> UserTrackedLocations { get; set; }

}