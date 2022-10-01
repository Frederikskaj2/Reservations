using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Core;

public record PostingsRange(LocalDate EarliestMonth, LocalDate LatestMonth);
