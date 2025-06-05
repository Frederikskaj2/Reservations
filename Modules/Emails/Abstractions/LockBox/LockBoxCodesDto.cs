using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Emails;

public record LockBoxCodesDto(
    OrderId OrderId,
    ResourceId ResourceId,
    LocalDate Date,
    IEnumerable<DatedLockBoxCode> DatedLockBoxCodes);
