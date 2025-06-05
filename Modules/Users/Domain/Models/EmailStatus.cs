namespace Frederikskaj2.Reservations.Users;

public record EmailStatus(EmailAddress Email, string NormalizedEmail, bool IsConfirmed);
