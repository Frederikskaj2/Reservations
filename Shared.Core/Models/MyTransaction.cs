using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Core;

public record MyTransaction(LocalDate Date, Activity Activity, OrderId? OrderId, string? Description, Amount Amount);
