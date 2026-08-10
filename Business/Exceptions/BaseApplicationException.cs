using System.Net;
using Business.Enum;

namespace Business.Exceptions;

public abstract class BaseApplicationException : Exception
{
    protected BaseApplicationException(string message, ExceptionCode errorCode,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest) :
        base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    protected BaseApplicationException(string message, ExceptionCode errorCode,
        Exception innerException, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public ExceptionCode ErrorCode { get; }

    /// <summary>
    ///     Status the API should return. These represent conditions the caller can act on, so they
    ///     default to 4xx rather than being reported as server faults.
    /// </summary>
    public HttpStatusCode StatusCode { get; }
}