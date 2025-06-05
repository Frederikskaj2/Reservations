namespace Frederikskaj2.Reservations.Users;

public enum SignUpError
{
    Unknown,
    InvalidRequest,
    TooShortPassword,
    ExposedPassword,
    EmailSameAsPassword,
}
