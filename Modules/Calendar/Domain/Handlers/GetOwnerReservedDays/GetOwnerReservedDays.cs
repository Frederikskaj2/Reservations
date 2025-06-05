using Frederikskaj2.Reservations.Orders;
using static Frederikskaj2.Reservations.Calendar.CalendarFunctions;

namespace Frederikskaj2.Reservations.Calendar;

static class GetOwnerReservedDays
{
    public static GetOwnerReservedDaysOutput GetOwnerReservedDaysCore(GetOwnerReservedDaysInput input) =>
        new(
            input.Reservations
                .FilterDateRange(input.Query.FromDate, input.Query.ToDate)
                .Bind(reservation => GetReservedDays(reservation, IsOwnerReservedDay(reservation))));

    static bool IsOwnerReservedDay(CalendarReservation reservation) =>
        reservation.OrderFlags.HasFlag(OrderFlags.IsOwnerOrder);
}
