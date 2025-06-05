using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record BankTransaction(
    BankTransactionId BankTransactionId,
    LocalDate Date,
    string Text,
    Amount Amount,
    Amount Balance,
    BankTransactionStatus Status) : IHasId
{
    public Option<TransactionId> ReconciledTransactionId { get; init; }

    public string GetId() => BankTransactionId.ToString();
}
