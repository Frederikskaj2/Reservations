using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record MonthRange(LocalDate EarliestMonth, LocalDate LatestMonth);
