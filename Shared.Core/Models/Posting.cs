using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record Posting(
    TransactionId TransactionId,
    LocalDate Date,
    Activity Activity,
    UserId? UserId,
    string FullName,
    OrderId? OrderId,
    IEnumerable<AccountAmount> Amounts);
