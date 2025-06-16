using Business.Enum;

namespace Business.Exceptions;

public abstract class BaseApplicationException : Exception
{
    protected BaseApplicationException(string message, ExceptionCode errorCode) :
        base(message)
    {
        ErrorCode = errorCode;
    }

    protected BaseApplicationException(string message, ExceptionCode errorCode,
        Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public ExceptionCode ErrorCode { get; }
}