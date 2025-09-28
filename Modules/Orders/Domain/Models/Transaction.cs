using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public sealed record Transaction(
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
    string IHasId.GetId() => TransactionId.GetId();
}
