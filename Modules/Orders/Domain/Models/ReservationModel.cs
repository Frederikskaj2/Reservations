using Frederikskaj2.Reservations.LockBox;

namespace Frederikskaj2.Reservations.Orders;

public record ReservationModel(ResourceId ResourceId, ResourceType ResourceType, Extent Extent);
