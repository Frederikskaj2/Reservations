using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record ImportResult(int Count, Option<LocalDate> LatestImportStartDate);
