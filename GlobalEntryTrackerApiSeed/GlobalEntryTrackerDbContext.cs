using GlobalEntryTrackerApiSeed.Entities;
using Microsoft.EntityFrameworkCore;

namespace GlobalEntryTrackerApiSeed;

public class GlobalEntryTrackerDbContext : DbContext
{
    public DbSet<GlobalEntryTrackerAppointmentLocationEntity> AppointmentLocations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //Get the connection string from the environment variable
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");

        var connectionString =
            $"Host={dbHost}:{dbPort};Username={dbUser};Password={dbPassword};Database={dbName};Include Error Detail=True";

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string is not set in the environment variable 'CONNECTION_STRING'.");
        }

        optionsBuilder.UseNpgsql(connectionString);
    }
}