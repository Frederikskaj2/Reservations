using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using static Frederikskaj2.Reservations.Calendar.CalendarFunctions;

namespace Frederikskaj2.Reservations.Calendar;

static class GetMyReservedDays
{
    public static GetMyReservedDaysOutput GetMyReservedDaysCore(GetMyReservedDaysInput input) =>
        new(
            input.Reservations
                .FilterDateRange(input.Query.FromDate, input.Query.ToDate)
                .Bind(reservation => GetReservedDays(reservation, IsMyReservedDay(input.Query.UserId, reservation))));

    static bool IsMyReservedDay(UserId userId, CalendarReservation reservation) =>
        reservation.UserId == userId && !reservation.OrderFlags.HasFlag(OrderFlags.IsOwnerOrder);
}
