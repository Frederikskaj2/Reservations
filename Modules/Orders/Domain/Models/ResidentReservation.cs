namespace Frederikskaj2.Reservations.Orders;

public record ResidentReservation(
    ResourceId ResourceId,
    ReservationStatus Status,
    Price Price,
    Extent Extent,
    bool CanBeCancelled,
    EntryCode? EntryCode);
