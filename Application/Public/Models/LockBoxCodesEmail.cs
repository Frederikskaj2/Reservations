using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Application;

public record LockBoxCodesEmail(
    EmailAddress Email,
    string FullName,
    OrderId OrderId,
    ResourceId ResourceId,
    LocalDate Date,
    IEnumerable<DatedLockBoxCode> Codes);
