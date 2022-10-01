using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record Reservation(
    ResourceId ResourceId,
    ReservationStatus Status,
    Price? Price,
    Extent Extent,
    bool CanBeCancelled,
    IEnumerable<DatedLockBoxCode>? LockBoxCodes);
