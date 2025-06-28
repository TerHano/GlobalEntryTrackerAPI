using Business.Enum;

namespace Business.Exceptions;

public class NotAvailableException(string message) : BaseApplicationException(
    message,
    ExceptionCode.NotAvailable);