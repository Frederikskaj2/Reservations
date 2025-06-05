namespace Frederikskaj2.Reservations.Users;

public enum PasswordValidationError
{
    None,
    TooShort,
    Exposed,
    SameAsEmail,
}
