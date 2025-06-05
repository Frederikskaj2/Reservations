using LanguageExt;

namespace Frederikskaj2.Reservations.Calendar;

record GetMyReservedDaysInput(GetMyReservedDaysQuery Query, Seq<CalendarReservation> Reservations);