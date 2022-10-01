using System.Collections.Generic;
using System.Linq;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.LineItemFactory;
using static Frederikskaj2.Reservations.Application.UserFactory;
using static Frederikskaj2.Reservations.Shared.Core.ReservationPolicies;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class OrderDetailsFactory
{
    public static EitherAsync<Failure, OrderDetails> CreateOrderDetails(
        OrderingOptions options, LocalDate today, IPersistenceContext context, OrderId orderId) =>
        CreateOrderDetails(options, today, context, context.Order(orderId), context.Item<User>());

    static EitherAsync<Failure, OrderDetails> CreateOrderDetails(
        OrderingOptions options, LocalDate today, IPersistenceContext context, Order order, User user) =>
        from userFullNames in ReadUserFullNames(context, toHashSet(order.Audits.Filter(audit => audit.UserId.HasValue).Map(audit => audit.UserId!.Value)))
        let userHashMap = toHashMap(userFullNames.Map(u => (u.UserId, u.FullName)))
        select CreateOrderDetails(options, today, order, user, userHashMap);

    static OrderDetails CreateOrderDetails(OrderingOptions options, LocalDate today, Order order, User user, HashMap<UserId, string> userFullNames) =>
        !order.Flags.HasFlag(OrderFlags.IsOwnerOrder)
            ? CreateUserOrderWithDetails(options, today, order, user, userFullNames)
            : CreateOwnerOrderWithDetails(order, user, userFullNames);

    static OrderDetails CreateUserOrderWithDetails(
        OrderingOptions options, LocalDate today, Order order, User user, HashMap<UserId, string> userFullNames) =>
        new(
            order.OrderId,
            OrderType.User,
            order.CreatedTimestamp,
            CreateUserInformation(user),
            CreateUserReservations(options, today, order.Reservations),
            order.Flags.HasFlag(OrderFlags.IsHistoryOrder),
            new(
                user.AccountNumber!,
                order.User!.NoFeeCancellationIsAllowedBefore,
                CreateLineItems(order.User)),
            null,
            order.Audits.Map(audit => CreateOrderAudit(audit, userFullNames)));

    static IEnumerable<Shared.Core.Reservation> CreateUserReservations(OrderingOptions options, LocalDate today, IEnumerable<Reservation> reservations) =>
        reservations.Select(reservation =>
            new Shared.Core.Reservation(
                reservation.ResourceId,
                reservation.Status,
                reservation.Price,
                reservation.Extent,
                CanReservationBeCancelled(options, today, reservation.Status, reservation.Extent, true),
                null));

    static OrderDetails CreateOwnerOrderWithDetails(Order order, User user, HashMap<UserId, string> userFullNames)
    {
        var reservations = CreateOwnerReservations(order.Reservations).ToList();
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
                order.Flags.HasFlag(OrderFlags.IsCleaningRequired)),
            order.Audits.Map(audit => CreateOrderAudit(audit, userFullNames)));
    }

    static IEnumerable<Shared.Core.Reservation> CreateOwnerReservations(IEnumerable<Reservation> reservations) =>
        reservations.Select(reservation =>
            new Shared.Core.Reservation(
                reservation.ResourceId,
                reservation.Status,
                reservation.Price,
                reservation.Extent,
                true,
                null));

    static Shared.Core.OrderAudit CreateOrderAudit(
        OrderAudit audit, HashMap<UserId, string> userFullNames) =>
        new(
            audit.Timestamp,
            audit.UserId,
            audit.UserId.HasValue ? userFullNames[audit.UserId.Value] : null,
            audit.Type,
            audit.TransactionId);
}
