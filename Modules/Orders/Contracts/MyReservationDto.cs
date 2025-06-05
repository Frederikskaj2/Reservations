using Frederikskaj2.Reservations.LockBox;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record MyReservationDto(
    ResourceId ResourceId,
    ReservationStatus Status,
    Price? Price,
    Extent Extent,
    bool CanBeCancelled,
    IEnumerable<DatedLockBoxCode> LockBoxCodes) : ReservationDto(ResourceId, Status, Price, Extent, CanBeCancelled);
