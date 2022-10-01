namespace Frederikskaj2.Reservations.Shared.Core;

public enum NewPasswordError
{
    Unknown,
    InvalidRequest,
    TokenExpired,
    TooShortPassword,
    ExposedPassword,
    EmailSameAsPassword
}
