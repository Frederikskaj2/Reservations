using Frederikskaj2.Reservations.LockBox;

namespace Frederikskaj2.Reservations.Orders;

public record ReservationDto(
    ResourceId ResourceId,
    ReservationStatus Status,
    Price? Price,
    Extent Extent,
    bool CanBeCancelled);
