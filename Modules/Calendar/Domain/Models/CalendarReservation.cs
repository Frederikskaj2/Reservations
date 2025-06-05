using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Calendar;

public record CalendarReservation(OrderId OrderId, UserId UserId, OrderFlags OrderFlags, ResourceId ResourceId, Extent Extent);
