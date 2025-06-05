using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Calendar;

public record GetMyReservedDaysQuery(Option<LocalDate> FromDate, Option<LocalDate> ToDate, UserId UserId);
