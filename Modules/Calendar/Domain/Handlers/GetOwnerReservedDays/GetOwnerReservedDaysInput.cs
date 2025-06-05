using LanguageExt;

namespace Frederikskaj2.Reservations.Calendar;

record GetOwnerReservedDaysInput(GetOwnerReservedDaysQuery Query, Seq<CalendarReservation> Reservations);
