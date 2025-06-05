using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record CreatePayOutCommand(Instant Timestamp, UserId ResidentId, Amount Amount);
