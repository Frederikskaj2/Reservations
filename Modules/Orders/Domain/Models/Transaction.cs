using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record Transaction(
    TransactionId TransactionId,
    LocalDate Date,
    UserId AdministratorId,
    Instant Timestamp,
    Activity Activity,
    UserId ResidentId,
    Option<TransactionDescription> Description,
    AccountAmounts Amounts)
    : IHasId
{
    public static string GetId(TransactionId transactionId) => transactionId.ToString()!;

    string IHasId.GetId() => TransactionId.GetId();
}
