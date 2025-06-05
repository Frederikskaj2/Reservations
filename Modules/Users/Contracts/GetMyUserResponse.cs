using Frederikskaj2.Reservations.Core;

namespace Frederikskaj2.Reservations.Users;

public record GetMyUserResponse(
    UserIdentityDto Identity,
    bool IsEmailConfirmed,
    Roles Roles,
    EmailSubscriptions EmailSubscriptions,
    bool IsPendingDelete,
    string? AccountNumber);
