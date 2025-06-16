using Business.Enum;

namespace Business.Exceptions;

public class ResendVerifyEmailException(string message)
    : BaseApplicationException(message, ExceptionCode.ResendVerifyEmail);