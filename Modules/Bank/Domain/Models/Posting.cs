using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record Posting(
    TransactionId TransactionId,
    LocalDate Date,
    Activity Activity,
    UserId ResidentId,
    Option<OrderId> OrderId,
    HashMap<PostingAccount, Amount> Amounts);
