using Frederikskaj2.Reservations.LockBox;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record ReservationDescription(ResourceId ResourceId, LocalDate Date);
