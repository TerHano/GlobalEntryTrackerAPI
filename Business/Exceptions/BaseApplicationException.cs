namespace GlobalEntryTrackerAPI.Exceptions;

public abstract class BaseApplicationException : Exception
{
    protected BaseApplicationException(string message, int errorCode) :
        base(message)
    {
        ErrorCode = errorCode;
    }

    protected BaseApplicationException(string message, int errorCode,
        Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public int ErrorCode { get; }
}