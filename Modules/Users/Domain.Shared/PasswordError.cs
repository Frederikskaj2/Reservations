namespace Frederikskaj2.Reservations.Users;

public enum PasswordError
{
    Unknown,
    InvalidRequest,
    WrongPassword,
    TooShortPassword,
    ExposedPassword,
    EmailSameAsPassword,
}
