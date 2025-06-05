using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Bank;

public record CreatePayOutRequest(UserId ResidentId, Amount Amount);
