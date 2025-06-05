using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record ReconcileCommand(Instant Timestamp, UserId AdministratorId, BankTransactionId BankTransactionId, UserId ResidentId);
