using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record ImportResult(int Count, DateRange DateRange, Option<LocalDate> LatestImportStartDate);
