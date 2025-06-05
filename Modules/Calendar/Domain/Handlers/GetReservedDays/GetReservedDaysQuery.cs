using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Calendar;

public record GetReservedDaysQuery(Option<LocalDate> FromDate, Option<LocalDate> ToDate);
