using Database.Entities;
using Database.Entities.NotificationSettings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class GlobalEntryTrackerDbContext(DbContextOptions<GlobalEntryTrackerDbContext> options)
    : IdentityDbContext<UserEntity, RoleEntity, string>(options)
{
    public DbSet<AppointmentLocationEntity> AppointmentLocations { get; set; }
    public DbSet<UserEntity> Users { get; set; }

    public DbSet<UserProfileEntity> UserProfiles { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<UserNotificationEntity> UserNotifications { get; set; }
    public DbSet<TrackedLocationForUserEntity> UserTrackedLocations { get; set; }
    public DbSet<DiscordNotificationSettingsEntity> DiscordNotifications { get; set; }
    public DbSet<NotificationTypeEntity> NotificationTypes { get; set; }
    public DbSet<DiscordNotificationSettingsEntity> DiscordNotificationSettings { get; set; }

    public DbSet<EmailNotificationSettingsEntity> EmailNotificationSettings { get; set; }

    public DbSet<UserCustomerEntity> UserCustomers { get; set; }

    public DbSet<PlanOptionEntity> PlanOptions { get; set; }

    public DbSet<ArchivedAppointmentEntity> ArchivedAppointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AppointmentLocationEntity>()
            .HasIndex(e => e.ExternalId).IsUnique();

        modelBuilder.Entity<ArchivedAppointmentEntity>()
            .HasOne(e => e.Location)
            .WithMany();

        modelBuilder.Entity<TrackedLocationForUserEntity>()
            .HasOne(e => e.Location)
            .WithMany()
            .HasForeignKey(e => e.LocationId);

        modelBuilder.Entity<TrackedLocationForUserEntity>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);

        modelBuilder.Entity<DiscordNotificationSettingsEntity>()
            .HasOne(d => d.UserNotification)
            .WithOne(u => u.DiscordNotificationSettings)
            .HasForeignKey<DiscordNotificationSettingsEntity>(d => d.UserNotificationId)
            .IsRequired();


        modelBuilder.Entity<EmailNotificationSettingsEntity>()
            .HasOne(d => d.UserNotification)
            .WithOne(u => u.EmailNotificationSettings)
            .HasForeignKey<EmailNotificationSettingsEntity>(d => d.UserNotificationId)
            .IsRequired();


        modelBuilder.Entity<UserNotificationEntity>()
            .HasOne(e => e.DiscordNotificationSettings)
            .WithOne(e => e.UserNotification)
            .HasForeignKey<UserNotificationEntity>(e => e.DiscordNotificationSettingsId);

        modelBuilder.Entity<UserNotificationEntity>()
            .HasOne(e => e.EmailNotificationSettings)
            .WithOne(e => e.UserNotification)
            .HasForeignKey<UserNotificationEntity>(e => e.EmailNotificationSettingsId);


        modelBuilder.Entity<UserNotificationEntity>()
            .HasOne(e => e.EmailNotificationSettings)
            .WithOne(e => e.UserNotification)
            .HasForeignKey<UserNotificationEntity>(e => e.EmailNotificationSettingsId);

        modelBuilder.Entity<UserProfileEntity>().Property(u => u.NextNotificationAt)
            .HasDefaultValue(DateTime.UtcNow);


        modelBuilder.Entity<UserCustomerEntity>()
            .HasOne(e => e.User);

        modelBuilder.Entity<UserEntity>()
            .HasMany(u => u.UserRoles)
            .WithMany(r => r.Users)
            .UsingEntity<IdentityUserRole<string>>();
    }
}