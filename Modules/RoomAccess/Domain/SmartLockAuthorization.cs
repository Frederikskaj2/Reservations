using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.RoomAccess;

public record SmartLockAuthorization(
    ResourceId ResourceId,
    OrderId OrderId,
    Instant FromTimestamp,
    Instant ToTimestamp,
    EntryCode EntryCode);
