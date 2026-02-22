namespace GlobalEntryTrackerApiSeed.Models;

public class AppointmentLocation
{
    public AppointmentLocation()
    {
        Services = new List<Service>();
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string LocationType { get; set; }
    public string Address { get; set; }
    public string AddressAdditional { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string CountryCode { get; set; }
    public string TzData { get; set; }
    public string PhoneNumber { get; set; }
    public string PhoneCountryCode { get; set; }
    public string PhoneExtension { get; set; }
    public string PhoneAltNumber { get; set; }
    public string PhoneAltCountryCode { get; set; }
    public string PhoneAltExtension { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool Temporary { get; set; }
    public bool InviteOnly { get; set; }
    public bool Operational { get; set; }
    public string Directions { get; set; }
    public string Notes { get; set; }
    public string MapFileName { get; set; }
    public string AccessCode { get; set; }
    public string LastUpdatedBy { get; }
    public DateTime LastUpdatedDate { get; }
    public DateTime CreatedDate { get; }
    public bool RemoteInd { get; set; }
    public List<Service> Services { get; set; }

    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}