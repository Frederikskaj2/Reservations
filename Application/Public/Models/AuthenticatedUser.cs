using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record AuthenticatedUser(
    Instant Timestamp,
    UserId UserId,
    EmailAddress Email,
    string FullName,
    Roles Roles,
    RefreshToken RefreshToken);
