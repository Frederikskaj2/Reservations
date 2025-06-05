using static Frederikskaj2.Reservations.Calendar.CalendarFunctions;

namespace Frederikskaj2.Reservations.Calendar;

static class GetReservedDays
{
    public static GetReservedDaysOutput GetReservedDaysCore(GetReservedDaysInput input) =>
        new(
            input.Reservations
                .FilterDateRange(input.Query.FromDate, input.Query.ToDate)
                .Bind(reservation => GetReservedDays(reservation, isMyReservedDay: false)));
}
