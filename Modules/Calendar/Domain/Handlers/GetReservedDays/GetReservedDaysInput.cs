using LanguageExt;

namespace Frederikskaj2.Reservations.Calendar;

record GetReservedDaysInput(GetReservedDaysQuery Query, Seq<CalendarReservation> Reservations);
