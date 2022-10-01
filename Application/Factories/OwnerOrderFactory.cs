using System.Collections.Generic;
using System.Linq;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using static Frederikskaj2.Reservations.Application.UserFactory;
using static Frederikskaj2.Reservations.Shared.Core.ReservationPolicies;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class OwnerOrderFactory
{
    public static IEnumerable<Shared.Core.Order> CreateOwnerOrders(LocalDate today, IEnumerable<Order> orders, IEnumerable<User> users) =>
        CreateOwnerOrders(today, orders, toHashMap(users.Map(user => (user.UserId, user))));

    static IEnumerable<Shared.Core.Order> CreateOwnerOrders(LocalDate today, IEnumerable<Order> orders, HashMap<UserId, User> users) =>
        from order in GetSortedOrders(orders)
        let user = users[order.UserId]
        select CreateOwnerOrder(today, order, user);

    static IEnumerable<Order> GetSortedOrders(IEnumerable<Order> orders) =>
        orders
            .OrderBy(order => order.Reservations.OrderBy(reservation => reservation.Extent.Date).First().Extent.Date)
            .ThenBy(order => order.OrderId);

    static Shared.Core.Order CreateOwnerOrder(LocalDate today, Order order, User user)
    {
        var reservations = CreateOwnerReservations(today, order.Reservations).ToList();
        return new(
            order.OrderId,
            OrderType.Owner,
            order.CreatedTimestamp,
            CreateUserInformation(user),
            reservations,
            order.Flags.HasFlag(OrderFlags.IsHistoryOrder),
            null,
            new(
                order.Owner!.Description,
                order.Flags.HasFlag(OrderFlags.IsCleaningRequired)));
    }

    static IEnumerable<Shared.Core.Reservation> CreateOwnerReservations(LocalDate today, IEnumerable<Reservation> reservations) =>
        reservations.Select(reservation =>
            new Shared.Core.Reservation(
                reservation.ResourceId,
                reservation.Status,
                reservation.Price,
                reservation.Extent,
                CanOwnerReservationBeCancelled(today, reservation.Status, reservation.Extent),
                null));
}
