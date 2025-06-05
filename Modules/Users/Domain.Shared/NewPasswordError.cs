namespace Frederikskaj2.Reservations.Users;

public enum NewPasswordError
{
    Unknown,
    InvalidRequest,
    TokenExpired,
    TooShortPassword,
    ExposedPassword,
    EmailSameAsPassword,
}
