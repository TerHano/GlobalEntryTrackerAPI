namespace Business.Exceptions;

public class IncorrectLoginInformationException(string message)
    : BaseApplicationException(message, 1002);