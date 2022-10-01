using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record ReservedDaysCommand(Option<LocalDate> FromDate, Option<LocalDate> ToDate);