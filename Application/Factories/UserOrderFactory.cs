using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using static Frederikskaj2.Reservations.Application.UserFactory;
using static Frederikskaj2.Reservations.Shared.Core.ReservationPolicies;

namespace Frederikskaj2.Reservations.Application;

static class UserOrderFactory
{
    public static Shared.Core.Order CreateUserOrder(OrderingOptions options, LocalDate today, Order order, IReadOnlyDictionary<UserId, User> users) =>
        CreateUserOrder(options, today, order, users[order.UserId]);

    static Shared.Core.Order CreateUserOrder(OrderingOptions options, LocalDate today, Order order, User user) =>
        new(
            order.OrderId,
            OrderType.User,
            order.CreatedTimestamp,
            CreateUserInformation(user),
            CreateUserReservations(options, today, order.Reservations),
            order.Flags.HasFlag(OrderFlags.IsHistoryOrder),
            new Shared.Core.UserOrder(
                user.AccountNumber!,
                order.User!.NoFeeCancellationIsAllowedBefore,
                LineItemFactory.CreateLineItems(order.User)),
            null);

    static IEnumerable<Shared.Core.Reservation> CreateUserReservations(OrderingOptions options, LocalDate today, IEnumerable<Reservation> reservations) =>
        reservations.Map(reservation =>
            new Shared.Core.Reservation(
                reservation.ResourceId,
                reservation.Status,
                reservation.Price,
                reservation.Extent,
                CanReservationBeCancelled(options, today, reservation.Status, reservation.Extent, true),
                null));
}
