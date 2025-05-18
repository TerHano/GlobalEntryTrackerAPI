namespace GlobalEntryTrackerAPI.Exceptions;

public class TrackerForLocationAndTypeExistsException : BaseApplicationException
{
    public TrackerForLocationAndTypeExistsException(string message) : base(message, 1001
    )
    {
    }

    public TrackerForLocationAndTypeExistsException(string message, Exception innerException) :
        base(message, 1001, innerException)
    {
    }
}