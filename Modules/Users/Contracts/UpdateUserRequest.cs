using Frederikskaj2.Reservations.Core;

namespace Frederikskaj2.Reservations.Users;

public record UpdateUserRequest(string? FullName, string? Phone, Roles Roles, bool IsPendingDelete);
