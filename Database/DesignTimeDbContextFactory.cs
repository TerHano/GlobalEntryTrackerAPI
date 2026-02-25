using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Database;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GlobalEntryTrackerDbContext>
{
    public GlobalEntryTrackerDbContext CreateDbContext(string[] args)
    {
        var basePath = ResolveBasePath();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var dbServer = configuration["Database:Server"];
        var dbPortRaw = configuration["Database:Port"];
        var dbName = configuration["Database:Name"];
        var dbUsername = configuration["Database:Username"];
        var dbPassword = configuration["Database:Password"];

        if (!int.TryParse(dbPortRaw, out var dbPort) ||
            string.IsNullOrWhiteSpace(dbServer) ||
            string.IsNullOrWhiteSpace(dbName) ||
            string.IsNullOrWhiteSpace(dbUsername) ||
            string.IsNullOrWhiteSpace(dbPassword))
        {
            throw new InvalidOperationException("Database configuration is missing. Ensure appsettings has Database:Server/Port/Name/Username/Password.");
        }

        var connectionString = string.Concat(
            "Host=", dbServer, ";",
            "Port=", dbPort.ToString(), ";",
            "Database=", dbName, ";",
            "Username=", dbUsername, ";",
            "Password=", dbPassword, ";");

        var optionsBuilder = new DbContextOptionsBuilder<GlobalEntryTrackerDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new GlobalEntryTrackerDbContext(optionsBuilder.Options);
    }

    private static string ResolveBasePath()
    {
        var current = Directory.GetCurrentDirectory();
        if (File.Exists(Path.Combine(current, "appsettings.json")))
            return current;

        var fromCurrent = Path.Combine(current, "GlobalEntryTrackerAPI");
        if (File.Exists(Path.Combine(fromCurrent, "appsettings.json")))
            return fromCurrent;

        var fromParent = Path.Combine(Directory.GetParent(current)?.FullName ?? current, "GlobalEntryTrackerAPI");
        if (File.Exists(Path.Combine(fromParent, "appsettings.json")))
            return fromParent;

        return current;
    }
}
