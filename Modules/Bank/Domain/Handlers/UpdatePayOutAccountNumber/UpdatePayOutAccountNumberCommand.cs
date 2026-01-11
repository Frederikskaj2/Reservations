using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record UpdatePayOutAccountNumberCommand(Instant Timestamp, PayOutId PayOutId, UserId UserId, AccountNumber AccountNumber);