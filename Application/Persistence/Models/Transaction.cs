using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

record Transaction(
    TransactionId TransactionId,
    LocalDate Date,
    UserId CreatedByUserId,
    Instant Timestamp,
    Activity Activity,
    UserId UserId,
    OrderId? OrderId,
    TransactionDescription? Description,
    AccountAmounts Amounts)
{
    public static string GetId(TransactionId transactionId) => transactionId.ToString();
}
