using Frederikskaj2.Reservations.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record UpdateUserCommand(
    Instant Timestamp,
    UserId AdministratorId,
    UserId UserId,
    string FullName,
    string Phone,
    Roles Roles,
    bool IsPendingDelete);
