using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record DateRange(LocalDate EarliestDate, LocalDate LatestDate);
