namespace Frederikskaj2.Reservations.Users;

public record UpdateMyUserRequest(string? FullName, string? Phone, EmailSubscriptions EmailSubscriptions);
