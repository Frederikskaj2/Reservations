using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record UpdateUserCommand(
    Instant Timestamp,
    UserId AdministratorUserId,
    UserId UserId,
    string FullName,
    string Phone,
    Roles Roles,
    bool IsPendingDelete);
