using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Core;

public record ReservationDescription(string ResourceName, LocalDate Date);
