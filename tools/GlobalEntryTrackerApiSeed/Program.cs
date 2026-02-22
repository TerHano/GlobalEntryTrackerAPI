using Database;
using GlobalEntryTrackerApiSeed.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Database
var connectionString = GetConnectionString(builder.Configuration);
builder.Services.AddDbContextFactory<GlobalEntryTrackerDbContext>(options =>
    options.UseNpgsql(connectionString));

// Services
builder.Services.AddHttpClient();
builder.Services.AddSingleton<AppointmentLocationSeederService>();

var host = builder.Build();

// Run the seeder
var seederService = host.Services.GetRequiredService<AppointmentLocationSeederService>();
var exitCode = await seederService.SeedLocationsAsync();

return exitCode;

static string GetConnectionString(IConfiguration configuration)
{
    // Prefer a full connection string if provided (useful for Docker/production)
    var envConnectionString = configuration["CONNECTION_STRING"] ?? 
                             Environment.GetEnvironmentVariable("CONNECTION_STRING");
    
    if (!string.IsNullOrWhiteSpace(envConnectionString))
    {
        return envConnectionString;
    }

    // Otherwise build from individual environment variables or configuration
    var dbHost = configuration["DB_HOST"] ?? Environment.GetEnvironmentVariable("DB_HOST");
    var dbPort = configuration["DB_PORT"] ?? Environment.GetEnvironmentVariable("DB_PORT");
    var dbUser = configuration["DB_USER"] ?? Environment.GetEnvironmentVariable("DB_USER");
    var dbPassword = configuration["DB_PASSWORD"] ?? Environment.GetEnvironmentVariable("DB_PASSWORD");
    var dbName = configuration["DB_NAME"] ?? Environment.GetEnvironmentVariable("DB_NAME");

    var missing = new List<string>();
    if (string.IsNullOrWhiteSpace(dbHost)) missing.Add("DB_HOST");
    if (string.IsNullOrWhiteSpace(dbPort)) missing.Add("DB_PORT");
    if (string.IsNullOrWhiteSpace(dbUser)) missing.Add("DB_USER");
    if (string.IsNullOrWhiteSpace(dbPassword)) missing.Add("DB_PASSWORD");
    if (string.IsNullOrWhiteSpace(dbName)) missing.Add("DB_NAME");

    if (missing.Count > 0)
    {
        throw new InvalidOperationException(
            $"Missing required database configuration: {string.Join(", ", missing)}");
    }

    return $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPassword};Database={dbName};Include Error Detail=true";
}
