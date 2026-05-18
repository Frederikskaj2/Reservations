using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record ReservedDay(LocalDate Date, ResourceId ResourceId);
