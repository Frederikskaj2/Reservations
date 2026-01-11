using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record Reconciled(Instant Timestamp, BankTransactionId BankTransactionId, TransactionId TransactionId);
