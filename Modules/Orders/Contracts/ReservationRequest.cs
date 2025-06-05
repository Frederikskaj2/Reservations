using Frederikskaj2.Reservations.LockBox;

namespace Frederikskaj2.Reservations.Orders;

public record ReservationRequest(ResourceId ResourceId, Extent Extent);
