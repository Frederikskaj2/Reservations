using LanguageExt;
using NodaTime;
using System;

namespace Frederikskaj2.Reservations.Calendar;

static class CalendarFunctions
{
    public static Seq<CalendarReservation> FilterDateRange(
        this Seq<CalendarReservation> reservations, Option<LocalDate> fromDate, Option<LocalDate> toDate) =>
        reservations
            .Filter(fromDate, (day, date) => date <= day.Extent.Ends())
            .Filter(toDate, (day, date) => day.Extent.Date <= date);

    static Seq<TItem> Filter<TItem, TValue>(this Seq<TItem> source, Option<TValue> option, Func<TItem, TValue, bool> predicate) =>
        option.Case switch
        {
            TValue value => source.Filter(x => predicate(x, value)),
            _ => source,
        };

    public static Seq<MyReservedDay> GetReservedDays(CalendarReservation reservation, bool isMyReservedDay) =>
        (0, reservation.Extent.Nights).ToSeq()
        .Map(i => new MyReservedDay(reservation.Extent.Date.PlusDays(i), reservation.ResourceId, reservation.OrderId, isMyReservedDay));
}
