using Business.Enum;

namespace Business.Exceptions;

public class EmailNotConfirmedException(string message)
    : BaseApplicationException(message, ExceptionCode.EmailNotConfirmed);