namespace Frederikskaj2.Reservations.Users;

public record NewPasswordRequest(string? Email, string? Token, string? NewPassword);
