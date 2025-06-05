using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record LockBoxCodesEmailModel(
    EmailAddress Email,
    string FullName,
    OrderId OrderId,
    ResourceId ResourceId,
    LocalDate Date,
    IEnumerable<DatedLockBoxCode> Codes);
