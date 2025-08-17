using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record ImportBankTransactionsCommand(Instant Timestamp, string Transactions);
