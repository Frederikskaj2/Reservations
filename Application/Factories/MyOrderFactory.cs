using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using static Frederikskaj2.Reservations.Application.LineItemFactory;
using static Frederikskaj2.Reservations.Application.LockBoxCodeFunctions;
using static Frederikskaj2.Reservations.Application.PaymentFunctions;
using static Frederikskaj2.Reservations.Application.UserFactory;
using static Frederikskaj2.Reservations.Shared.Core.ReservationPolicies;

namespace Frederikskaj2.Reservations.Application;

static class MyOrderFactory
{
    public static IEnumerable<MyOrder> CreateMyOrders(
        OrderingOptions options, LocalDate today, LockBoxCodes lockBoxCodes, IEnumerable<Order> orders, User user) =>
        orders.Map(order => CreateMyOrder(options, today, lockBoxCodes, order, user));

    public static MyOrder CreateMyOrder(
        OrderingOptions options, LocalDate today, LockBoxCodes lockBoxCodes, Order order, User user) =>
        CreateMyOrder(
            options,
            today,
            CreateLockBoxCodeMap(lockBoxCodes),
            order,
            user,
            order.NeedsConfirmation() ? GetPaymentInformation(options, user) : null);

    public static MyOrder CreateMyOrder(
        OrderingOptions options, LocalDate today, LockBoxCodes lockBoxCodes, Order order, User user, PaymentInformation? payment) =>
        CreateMyOrder(options, today, CreateLockBoxCodeMap(lockBoxCodes), order, user, payment);

    static MyOrder CreateMyOrder(
        OrderingOptions options, LocalDate today, HashMap<(ResourceId, LocalDate), LockBoxCode> lockBoxCodes, Order order, User user,
        PaymentInformation? payment) =>
        CreateMyOrder(
            order,
            user,
            CreateReservations(
                options,
                today,
                lockBoxCodes,
                order.Reservations,
                order.UpdatedTimestamp() <= order.User!.NoFeeCancellationIsAllowedBefore),
            payment);

    static MyOrder CreateMyOrder(Order order, User user, Seq<Shared.Core.Reservation> reservations, PaymentInformation? payment) =>
        new(
            order.OrderId,
            order.CreatedTimestamp,
            reservations,
            order.Flags.HasFlag(OrderFlags.IsHistoryOrder),
            !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && reservations.Any(reservation => reservation.CanBeCancelled),
            order.Price(),
            order.User!.NoFeeCancellationIsAllowedBefore,
            payment,
            CreateLineItems(order.User),
            CreateUserInformation(user));

    static Seq<Shared.Core.Reservation> CreateReservations(
        OrderingOptions options, LocalDate today, HashMap<(ResourceId, LocalDate), LockBoxCode> lockBoxCodes, Seq<Reservation> reservations,
        bool noFeeCancellationIsAllowed) =>
        reservations.Map(reservation =>
            new Shared.Core.Reservation(
                reservation.ResourceId,
                reservation.Status,
                reservation.Price,
                reservation.Extent,
                CanReservationBeCancelled(options, today, reservation.Status, reservation.Extent, noFeeCancellationIsAllowed),
                CreateLockBoxCodes(options, lockBoxCodes, today, reservation)));

    static IEnumerable<DatedLockBoxCode> CreateLockBoxCodes(
        OrderingOptions options, HashMap<(ResourceId, LocalDate), LockBoxCode> lockBoxCodes, LocalDate today, Reservation reservation) =>
        reservation.Status is ReservationStatus.Confirmed && reservation.Extent.Date.PlusDays(-options.RevealLockBoxCodeDaysBeforeReservationStart) <= today
            ? CreateDatedLockBoxCodes(lockBoxCodes, reservation)
            : Enumerable.Empty<DatedLockBoxCode>();
}
