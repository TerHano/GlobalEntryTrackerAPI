using Business.Enum;

namespace Business.Exceptions;

public class IncorrectLoginInformationException(string message)
    : BaseApplicationException(message, ExceptionCode.IncorrectLoginInformation);