using Frederikskaj2.Reservations.LockBox;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record ReservedDay(LocalDate Date, ResourceId ResourceId);
