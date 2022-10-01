namespace Frederikskaj2.Reservations.Shared.Core;

public enum SignUpError
{
    Unknown,
    InvalidRequest,
    TooShortPassword,
    ExposedPassword,
    EmailSameAsPassword
}
