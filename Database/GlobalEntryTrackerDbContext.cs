using Database.Entities;
using Database.Entities.NotificationSettings;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class GlobalEntryTrackerDbContext(DbContextOptions<GlobalEntryTrackerDbContext> options)
    : DbContext(options)
{
    public DbSet<AppointmentLocationEntity> AppointmentLocations { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<TrackedLocationForUserEntity> UserTrackedLocations { get; set; }
    public DbSet<DiscordNotificationSettingsEntity> DiscordNotifications { get; set; }
    public DbSet<NotificationTypeEntity> NotificationTypes { get; set; }
    public DbSet<DiscordNotificationSettingsEntity> DiscordNotificationSettings { get; set; }

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

        modelBuilder.Entity<UserEntity>()
            .HasOne(e => e.DiscordNotificationSettings)
            .WithOne(e => e.User)
            .HasForeignKey<UserEntity>(e => e.DiscordNotificationSettingsId);
    }
}