namespace Frederikskaj2.Reservations.Users;

public record SignInRequest(string? Email, string? Password, bool IsPersistent);
