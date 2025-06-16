using Business.Enum;

namespace Business.Exceptions;

public class TrackerForLocationAndTypeExistsException(string message) : BaseApplicationException(
    message,
    ExceptionCode.TrackerForLocationAndTypeExists);