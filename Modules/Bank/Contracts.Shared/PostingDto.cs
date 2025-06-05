using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Bank;

public record PostingDto(
    TransactionId TransactionId,
    LocalDate Date,
    Activity Activity,
    UserId? ResidentId,
    string FullName,
    PaymentId PaymentId,
    OrderId? OrderId,
    IEnumerable<AccountAmountDto> Amounts);
