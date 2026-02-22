using System.Text.Json;
using Database;
using Database.Entities;
using GlobalEntryTrackerApiSeed.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GlobalEntryTrackerApiSeed.Services;

public class AppointmentLocationSeederService(
    IHttpClientFactory httpClientFactory,
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory,
    ILogger<AppointmentLocationSeederService> logger)
{
    private const string ApiUrl =
        "https://ttp.cbp.dhs.gov/schedulerapi/locations/?temporary=false&inviteOnly=false&operational=true&serviceName=Global%20Entry";

    public async Task<int> SeedLocationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting appointment location seeding process");

            var locations = await FetchLocationsFromApiAsync(cancellationToken);
            if (locations == null || locations.Count == 0)
            {
                logger.LogWarning("No appointment locations returned from API");
                return 1;
            }

            logger.LogInformation("Fetched {Count} locations from API", locations.Count);

            var entities = TransformLocations(locations);
            var (created, updated) = await SaveLocationsToDatabase(entities, cancellationToken);

            logger.LogInformation("Seeding completed. Created: {Created}, Updated: {Updated}", created, updated);
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during seeding");
            return 1;
        }
    }

    private async Task<List<AppointmentLocation>?> FetchLocationsFromApiAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching data from API: {ApiUrl}", ApiUrl);

        var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        var response = await httpClient.GetAsync(ApiUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        var locations = JsonSerializer.Deserialize<List<AppointmentLocation>>(
            jsonResponse,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return locations;
    }

    private List<AppointmentLocationEntity> TransformLocations(List<AppointmentLocation> locations)
    {
        var entities = new List<AppointmentLocationEntity>();

        foreach (var location in locations.Where(l =>
                     string.Equals(l.CountryCode, "US", StringComparison.OrdinalIgnoreCase) && !l.Temporary))
        {
            var entity = MapToEntity(location);
            if (entity != null)
            {
                entities.Add(entity);
            }
        }

        logger.LogInformation("Transformed {Count} valid locations for database", entities.Count);
        return entities;
    }

    private async Task<(int created, int updated)> SaveLocationsToDatabase(
        List<AppointmentLocationEntity> entities,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var createdCount = 0;
        var updatedCount = 0;

        foreach (var entity in entities)
        {
            var existing = await context.AppointmentLocations
                .FirstOrDefaultAsync(x => x.ExternalId == entity.ExternalId, cancellationToken);

            if (existing != null)
            {
                // Update properties
                existing.Name = entity.Name;
                existing.Address = entity.Address;
                existing.AddressAdditional = entity.AddressAdditional;
                existing.City = entity.City;
                existing.State = entity.State;
                existing.PostalCode = entity.PostalCode;
                existing.Timezone = entity.Timezone;
                updatedCount++;
            }
            else
            {
                context.AppointmentLocations.Add(entity);
                createdCount++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return (createdCount, updatedCount);
    }

    private AppointmentLocationEntity? MapToEntity(AppointmentLocation location)
    {
        // Only map if required fields exist
        var name = SafeTrimAndTruncate(location.Name, 100);
        var address = SafeTrimAndTruncate(location.Address, 100);
        var city = SafeTrimAndTruncate(location.City, 100);
        var state = SafeTrimAndTruncate(location.State, 100);
        var postal = SafeTrimAndTruncate(location.PostalCode, 15);

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address) ||
            string.IsNullOrEmpty(city) || string.IsNullOrEmpty(state) ||
            string.IsNullOrEmpty(postal))
        {
            logger.LogDebug("Skipping location Id={LocationId} due to missing required fields", location.Id);
            return null;
        }

        return new AppointmentLocationEntity
        {
            ExternalId = location.Id,
            Name = name,
            Address = address,
            AddressAdditional = SafeTrimAndTruncate(location.AddressAdditional, 100),
            City = city,
            State = state,
            PostalCode = postal,
            Timezone = SafeTrimAndTruncate(location.TzData, 100)
        };
    }

    private static string SafeTrimAndTruncate(string? input, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var trimmed = input.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}



