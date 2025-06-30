using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record ImportResult(int Count, Option<DateRange> DateRange, Option<LocalDate> LatestImportStartDate);
