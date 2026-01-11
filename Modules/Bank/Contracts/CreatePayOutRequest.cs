using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Bank;

public record CreatePayOutRequest(UserId ResidentId, string? AccountNumber, Amount Amount);
