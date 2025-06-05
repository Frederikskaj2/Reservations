using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record ReconcilePayOutCommand(Instant Timestamp, UserId AdministratorId, BankTransactionId BankTransactionId, PayOutId PayOutId);
