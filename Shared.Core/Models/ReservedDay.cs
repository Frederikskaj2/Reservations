using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Core;

public record ReservedDay(LocalDate Date, ResourceId ResourceId);
