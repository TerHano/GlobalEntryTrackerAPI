using System.Text.Json;
using Database;
using Database.Entities;
using GlobalEntryTrackerApiSeed.Models;
using GlobalEntryTrackerApiSeed.Services;
using Microsoft.AspNetCore.Identity;
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

// Identity
builder.Services.AddIdentityCore<UserEntity>()
    .AddRoles<RoleEntity>()
    .AddEntityFrameworkStores<GlobalEntryTrackerDbContext>()
    .AddUserManager<UserManager<UserEntity>>()
    .AddRoleManager<RoleManager<RoleEntity>>();

// Services
builder.Services.AddHttpClient();
builder.Services.AddSingleton<AppointmentLocationSeederService>();
builder.Services.AddSingleton<StripeCatalogSeederService>();
builder.Services.AddSingleton<StripeSubscriberBackfillService>();
builder.Services.AddSingleton<NotificationTypeSeederService>();
builder.Services.AddScoped<AdminUserSeederService>();
builder.Services.AddScoped<RoleSeederService>();

var host = builder.Build();

var runLocationSeed = args.Length == 0 ||
                      args.Contains("--seed-locations", StringComparer.OrdinalIgnoreCase);
var runStripeCatalogSeed =
    args.Contains("--seed-stripe-catalog", StringComparer.OrdinalIgnoreCase);
var runStripeSubscriberBackfill =
    args.Contains("--backfill-stripe-subscribers", StringComparer.OrdinalIgnoreCase);
var runNotificationTypeSeed =
    args.Contains("--seed-notification-types", StringComparer.OrdinalIgnoreCase);
var runAdminUserSeed =
    args.Contains("--seed-admin-user", StringComparer.OrdinalIgnoreCase);
var runRoleSeed =
    args.Contains("--seed-roles", StringComparer.OrdinalIgnoreCase);
var dryRunFromArgs = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
var dryRun = dryRunFromArgs || ParseBool(
    builder.Configuration["DRY_RUN"] ?? Environment.GetEnvironmentVariable("DRY_RUN"),
    false);

if (!runLocationSeed && !runStripeCatalogSeed && !runStripeSubscriberBackfill &&
    !runNotificationTypeSeed && !runAdminUserSeed && !runRoleSeed)
{
    var logger = host.Services.GetRequiredService<ILoggerFactory>()
        .CreateLogger("Program");
    logger.LogError(
        "No valid mode was provided. Use --seed-locations, --seed-stripe-catalog, --backfill-stripe-subscribers, --seed-notification-types, --seed-admin-user, and/or --seed-roles.");
    return 1;
}

var exitCode = 0;
LocationSeedResult? locationSeedResult = null;
StripeCatalogSeedResult? stripeCatalogSeedResult = null;
StripeSubscriberBackfillResult? stripeSubscriberBackfillResult = null;
NotificationTypeSeedResult? notificationTypeSeedResult = null;
AdminUserSeedResult? adminUserSeedResult = null;
RoleSeedResult? roleSeedResult = null;

if (runLocationSeed)
{
    var locationSeederService = host.Services
        .GetRequiredService<AppointmentLocationSeederService>();
    locationSeedResult = await locationSeederService.SeedLocationsAsync(dryRun);
    if (!locationSeedResult.Success)
    {
        exitCode = 1;
    }
}

if (runStripeCatalogSeed)
{
    var stripeCatalogSeederService = host.Services
        .GetRequiredService<StripeCatalogSeederService>();
    var stripeOptions = GetStripeCatalogSeedOptions(builder.Configuration, dryRun);
    stripeCatalogSeedResult = await stripeCatalogSeederService.SeedStripeCatalogAsync(
        stripeOptions);
    if (!stripeCatalogSeedResult.Success)
    {
        exitCode = 1;
    }
}

if (runStripeSubscriberBackfill)
{
    var stripeSubscriberBackfillService = host.Services
        .GetRequiredService<StripeSubscriberBackfillService>();
    var backfillOptions = GetStripeSubscriberBackfillOptions(builder.Configuration, dryRun);
    stripeSubscriberBackfillResult = await stripeSubscriberBackfillService.BackfillSubscribersAsync(
        backfillOptions);
    if (!stripeSubscriberBackfillResult.Success)
    {
        exitCode = 1;
    }
}

if (runNotificationTypeSeed)
{
    var notificationTypeSeederService = host.Services
        .GetRequiredService<NotificationTypeSeederService>();
    notificationTypeSeedResult =
        await notificationTypeSeederService.SeedNotificationTypesAsync(dryRun);
    if (!notificationTypeSeedResult.Success)
    {
        exitCode = 1;
    }
}

if (runAdminUserSeed)
{
    using var scope = host.Services.CreateScope();
    var adminUserSeederService = scope.ServiceProvider
        .GetRequiredService<AdminUserSeederService>();
    var adminOptions = GetAdminUserSeedOptions(builder.Configuration, dryRun);
    adminUserSeedResult = await adminUserSeederService.SeedAdminUserAsync(adminOptions);
    if (!adminUserSeedResult.Success)
    {
        exitCode = 1;
    }
}

if (runRoleSeed)
{
    using var scope = host.Services.CreateScope();
    var roleSeederService = scope.ServiceProvider
        .GetRequiredService<RoleSeederService>();
    roleSeedResult = await roleSeederService.SeedRolesAsync(dryRun);
    if (!roleSeedResult.Success)
    {
        exitCode = 1;
    }
}

if (dryRun)
{
    var reportOptions = GetReportOutputOptions(builder.Configuration);
    var report = new SeedReport
    {
        GeneratedAtUtc = DateTime.UtcNow,
        DryRun = true,
        LocationSeed = locationSeedResult,
        StripeCatalogSeed = stripeCatalogSeedResult,
        StripeSubscriberBackfill = stripeSubscriberBackfillResult,
        NotificationTypeSeed = notificationTypeSeedResult,
        AdminUserSeed = adminUserSeedResult,
        RoleSeed = roleSeedResult
    };

    var logger = host.Services.GetRequiredService<ILoggerFactory>()
        .CreateLogger("Program");
    var reportJson = JsonSerializer.Serialize(report,
        new JsonSerializerOptions
        {
            WriteIndented = true
        });
    var reportFullPath = Path.GetFullPath(reportOptions.ReportPath);
    var reportDirectory = Path.GetDirectoryName(reportFullPath);
    if (!string.IsNullOrWhiteSpace(reportDirectory))
    {
        Directory.CreateDirectory(reportDirectory);
    }

    await File.WriteAllTextAsync(reportFullPath, reportJson);
    logger.LogInformation("Dry-run report written to {ReportPath}", reportFullPath);
}

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
    var dbPassword = configuration["DB_PASSWORD"] ??
                     Environment.GetEnvironmentVariable("DB_PASSWORD");
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

    return
        $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPassword};Database={dbName};Include Error Detail=true";
}

static StripeCatalogSeedOptions GetStripeCatalogSeedOptions(
    IConfiguration configuration,
    bool dryRun)
{
    var stripeSecretKey =
        configuration["STRIPE_SECRET_KEY"] ?? Environment.GetEnvironmentVariable(
            "STRIPE_SECRET_KEY");
    var stripeProductIds = ParseCsvSet(
        configuration["STRIPE_PRODUCT_IDS"] ?? Environment.GetEnvironmentVariable(
            "STRIPE_PRODUCT_IDS"));
    var stripePriceIds = ParseCsvSet(
        configuration["STRIPE_PRICE_IDS"] ?? Environment.GetEnvironmentVariable(
            "STRIPE_PRICE_IDS"));
    var onlyActive = ParseBool(
        configuration["STRIPE_ONLY_ACTIVE"] ?? Environment.GetEnvironmentVariable(
            "STRIPE_ONLY_ACTIVE"),
        true);

    return new StripeCatalogSeedOptions
    {
        StripeSecretKey = stripeSecretKey,
        ProductIds = stripeProductIds,
        PriceIds = stripePriceIds,
        OnlyActive = onlyActive,
        DryRun = dryRun
    };
}

static HashSet<string> ParseCsvSet(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return [];
    }

    var parsedValues = value
        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
    return parsedValues;
}

static bool ParseBool(string? value, bool defaultValue)
{
    return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
}

static StripeSubscriberBackfillOptions GetStripeSubscriberBackfillOptions(
    IConfiguration configuration,
    bool dryRun)
{
    var stripeSecretKey =
        configuration["STRIPE_SECRET_KEY"] ?? Environment.GetEnvironmentVariable(
            "STRIPE_SECRET_KEY");
    var statuses = ParseCsvSet(
        configuration["STRIPE_BACKFILL_STATUSES"] ?? Environment.GetEnvironmentVariable(
            "STRIPE_BACKFILL_STATUSES"));
    if (statuses.Count == 0)
    {
        statuses = ["active", "trialing"];
    }

    var matchByEmail = ParseBool(
        configuration["STRIPE_BACKFILL_MATCH_EMAIL"] ?? Environment.GetEnvironmentVariable(
            "STRIPE_BACKFILL_MATCH_EMAIL"),
        true);
    return new StripeSubscriberBackfillOptions
    {
        StripeSecretKey = stripeSecretKey,
        SubscriptionStatuses = statuses,
        MatchByEmail = matchByEmail,
        DryRun = dryRun
    };
}

static ReportOutputOptions GetReportOutputOptions(IConfiguration configuration)
{
    var reportPath = configuration["REPORT_PATH"] ??
                     Environment.GetEnvironmentVariable("REPORT_PATH");
    if (string.IsNullOrWhiteSpace(reportPath))
    {
        reportPath = "seed-report.json";
    }

    return new ReportOutputOptions
    {
        ReportPath = reportPath
    };
}

static AdminUserSeedOptions GetAdminUserSeedOptions(IConfiguration configuration, bool dryRun)
{
    var email = configuration["ADMIN_EMAIL"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL");
    var password = configuration["ADMIN_PASSWORD"] ??
                   Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
    var firstName = configuration["ADMIN_FIRST_NAME"] ??
                    Environment.GetEnvironmentVariable("ADMIN_FIRST_NAME");
    var lastName = configuration["ADMIN_LAST_NAME"] ??
                   Environment.GetEnvironmentVariable("ADMIN_LAST_NAME");

    return new AdminUserSeedOptions
    {
        Email = email ?? string.Empty,
        Password = password ?? string.Empty,
        FirstName = firstName,
        LastName = lastName,
        DryRun = dryRun
    };
}