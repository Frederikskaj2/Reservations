using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record CancelPayOutCommand(Instant Timestamp, PayOutId PayOutId, UserId UserId);