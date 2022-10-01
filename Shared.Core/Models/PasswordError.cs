namespace Frederikskaj2.Reservations.Shared.Core;

public enum PasswordError
{
    Unknown,
    InvalidRequest,
    WrongPassword,
    TooShortPassword,
    ExposedPassword,
    EmailSameAsPassword
}
