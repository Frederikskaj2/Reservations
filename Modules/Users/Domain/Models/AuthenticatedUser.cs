using Frederikskaj2.Reservations.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record AuthenticatedUser(
    Instant Timestamp,
    UserId UserId,
    EmailAddress Email,
    string FullName,
    Roles Roles,
    RefreshToken RefreshToken);
