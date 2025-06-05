using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record GetYearlySummaryInput(GetYearlySummaryQuery Query, Seq<Reservation> Reservations);
