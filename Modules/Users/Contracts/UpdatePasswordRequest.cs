namespace Frederikskaj2.Reservations.Users;

public record UpdatePasswordRequest(string? CurrentPassword, string? NewPassword);
