using BusinessLayer;

namespace GlobalEntryTrackerAPI.Endpoints;

public static class LocationTrackerEndpoints
{
    public static void MapLocationTrackerEndpoints(this WebApplication app)
    {
        app.MapGet("/products", (UserAppointmentTrackerBusiness userAppointmentTrackerBusiness) => "List of products");
        app.MapGet("/products/{id}", (int id) => $"Product with ID: {id}");
    }
}
