using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.MyReservedDayFactory;

namespace Frederikskaj2.Reservations.Application;

public static class GetMyReservedDaysHandler
{
    public static EitherAsync<Failure, IEnumerable<MyReservedDay>> Handle(IPersistenceContextFactory contextFactory, UserReservedDaysCommand command) =>
        from orders in ReadOrders(CreateContext(contextFactory))
        select orders.Bind(
            order => order.Reservations
                .FilterReservations(status => status is ReservationStatus.Reserved or ReservationStatus.Confirmed, command)
                .Bind(reservation =>
                    CreateMyReservedDays(order.OrderId, reservation, order.UserId == command.UserId && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))));
}
