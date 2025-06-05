using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

record GetYearlySummaryRangeInput(GetYearlySummaryRangeQuery Query, Option<LocalDate> EarliestReservationDate);
