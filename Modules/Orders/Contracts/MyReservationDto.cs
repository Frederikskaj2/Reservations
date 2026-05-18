namespace Frederikskaj2.Reservations.Orders;

public record MyReservationDto(
    ResourceId ResourceId,
    ReservationStatus Status,
    Price? Price,
    Extent Extent,
    bool CanBeCancelled,
    EntryCode? EntryCode) : ReservationDto(ResourceId, Status, Price, Extent, CanBeCancelled, EntryCode);
