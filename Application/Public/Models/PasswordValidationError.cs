namespace Frederikskaj2.Reservations.Application;

public enum PasswordValidationError
{
    None,
    TooShort,
    Exposed,
    SameAsEmail
}