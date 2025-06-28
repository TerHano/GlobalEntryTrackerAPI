using System.Text.Json.Serialization;

namespace Business.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExceptionCode
{
    GenericError = 0,
    NotAvailable = 100,
    IncorrectLoginInformation = 1002,
    TrackerForLocationAndTypeExists = 1001,
    ResendVerifyEmail = 1003,
    EmailNotConfirmed = 1004
}