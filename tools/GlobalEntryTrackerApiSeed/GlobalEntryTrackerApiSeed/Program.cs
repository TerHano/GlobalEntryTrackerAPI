// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using GlobalEntryTrackerApiSeed;
using GlobalEntryTrackerApiSeed.Entities;
using GlobalEntryTrackerApiSeed.Models;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Fetching data from API...");

var apiUrl = "https://ttp.cbp.dhs.gov/schedulerapi/locations/?temporary=false&inviteOnly=false&operational=true&serviceName=Global%20Entry";
using var httpClient = new HttpClient();

try
{
    // Make the GET request
    var response = await httpClient.GetAsync(apiUrl);
    response.EnsureSuccessStatusCode();

    // Read and deserialize the JSON response
    var jsonResponse = await response.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var appointmentLocations =
        JsonSerializer.Deserialize<List<AppointmentLocation>>(jsonResponse, options);

    Console.WriteLine($"Fetched {appointmentLocations?.Count} locations.");


    // Transform the data to GlobalEntryTrackerAppointmentLocationEntity and save to database
    var appointmentLocationEntities = appointmentLocations
        ?.Where(location => location.CountryCode.Equals("US") && !location.Temporary)
        .Select<AppointmentLocation, GlobalEntryTrackerAppointmentLocationEntity>(location =>
            new GlobalEntryTrackerAppointmentLocationEntity
            {
                ExternalId = location.Id,
                Name = location.Name.Trim(),
                Address = location.Address.Trim(),
                AddressAdditional = location.AddressAdditional.Trim(),
                City = location.City.Trim(),
                State = location.State.Trim(),
                PostalCode = location.PostalCode.Trim(),
                Timezone = location.TzData.Trim()
                
            }).ToList();


    var createdCount = 0;
    var updatedCount = 0;
    await using var context = new GlobalEntryTrackerDbContext();
    if (appointmentLocationEntities != null)
    {
        foreach (var entity in appointmentLocationEntities)
        {
            var existing = await context.AppointmentLocations
                .FirstOrDefaultAsync(x => x.ExternalId == entity.ExternalId);

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
        await context.SaveChangesAsync();
        Console.WriteLine($"Updated {updatedCount}, Inserted {createdCount} locations to the database.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
