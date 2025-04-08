using Database.Entities;

namespace Database.Repositories;

public class AppointmentLocationRepository(GlobalEntryTrackerDbContext context)
{
    public List<AppointmentLocationEntity> GetAllAppointmentLocations()
    {
        return context.AppointmentLocations.ToList();
    }
    public AppointmentLocationEntity GetAppointmentLocationByExternalId(int id)
    {
        var appointmentLocation = context.AppointmentLocations.FirstOrDefault(x => x.ExternalId == id);
        if (appointmentLocation == null)
        {
            throw new NullReferenceException("Appointment location not found");
        }
        return appointmentLocation;
    }

    public void AddAppointmentLocation(AppointmentLocationEntity appointmentLocation)
    {
        context.AppointmentLocations.Add(appointmentLocation);
        context.SaveChanges();
    }
}
