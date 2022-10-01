using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Core;

public record MyReservedDay(LocalDate Date, ResourceId ResourceId, OrderId? OrderId, bool IsMyReservation);
