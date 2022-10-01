namespace Frederikskaj2.Reservations.Shared.Core;

public record MyUser(
    UserInformation Information,
    bool IsEmailConfirmed,
    Roles Roles,
    EmailSubscriptions EmailSubscriptions,
    bool IsPendingDelete,
    string? AccountNumber);
