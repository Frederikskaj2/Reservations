using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Calendar;

public record GetOwnerReservedDaysQuery(LocalDate Today, Option<LocalDate> FromDate, Option<LocalDate> ToDate);
