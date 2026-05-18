using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.RoomAccess;

public record ReservationInformation(OrderId OrderId, ResourceId ResourceId, Extent Extent, EntryCode EntryCode);
