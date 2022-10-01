using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Net;

namespace Frederikskaj2.Reservations.Application;

static class OrderFunctions
{
    public static EitherAsync<Failure, Order> GetUserOrder(IPersistenceContext context, OrderId orderId) =>
        context.OrderOption(orderId).Case switch
        {
            Order order => ValidateUserOrder(order),
            _ => Failure.New(HttpStatusCode.NotFound, $"Order {orderId} does not exist.")
        };

    static EitherAsync<Failure, Order> ValidateUserOrder(Order order) =>
        !order.Flags.HasFlag(OrderFlags.IsOwnerOrder)
            ? order
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Order {order.OrderId} is not a user order.");

    public static EitherAsync<Failure, Order> GetOwnerOrder(IPersistenceContext context, OrderId orderId) =>
        context.OrderOption(orderId).Case switch
        {
            Order order => ValidateOwnerOrder(order),
            _ => Failure.New(HttpStatusCode.NotFound, $"Order {orderId} does not exist.")
        };

    static EitherAsync<Failure, Order> ValidateOwnerOrder(Order order) =>
        order.Flags.HasFlag(OrderFlags.IsOwnerOrder)
            ? order
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Order {order.OrderId} is not an owner order.");

    public static IEnumerable<Reservation> GetReservations(IEnumerable<Order> orders) =>
        orders.Bind(order => order.Reservations.Filter(reservation => reservation.Status is ReservationStatus.Reserved or ReservationStatus.Confirmed));

    public static IEnumerable<ReservationWithOrder> GetUpcomingReservations(IEnumerable<Order> orders, LocalDate date) =>
        orders.Bind(
            order => order.Reservations
                .Map((index, reservation) => new ReservationWithOrder(reservation, order, index))
                .Filter(
                    reservationWithOrder =>
                        reservationWithOrder.Reservation.Status is ReservationStatus.Confirmed &&
                        !reservationWithOrder.Reservation.SentEmails.HasFlag(ReservationEmails.LockBoxCode) &&
                        reservationWithOrder.Reservation.Extent.Ends() < date));

    public static IEnumerable<ReservationWithOrder> GetReservationsToSettle(IEnumerable<Order> orders, LocalDate date) =>
        orders.Bind(
            order => order.Reservations
                .Map((index, reservation) => new ReservationWithOrder(reservation, order, index))
                .Filter(
                    reservationWithOrder =>
                        reservationWithOrder.Reservation.Status is ReservationStatus.Confirmed &&
                        !reservationWithOrder.Reservation.SentEmails.HasFlag(ReservationEmails.NeedsSettlement) &&
                        reservationWithOrder.Reservation.Cleaning!.End.Date < date));

    public static IPersistenceContext SetReservationEmailFlag(IPersistenceContext context, Seq<ReservationWithOrder> reservations, ReservationEmails flag) =>
        reservations.Fold(context, (context1, reservation) => SetReservationEmailFlag(context1, reservation, flag));

    static IPersistenceContext SetReservationEmailFlag(IPersistenceContext context, ReservationWithOrder reservation, ReservationEmails flag) =>
        context.UpdateItem<Order>(Order.GetId(reservation.Order.OrderId), order => SetReservationEmailFlag(order, reservation.Reservation, flag));

    static Order SetReservationEmailFlag(Order order, Reservation reservation, ReservationEmails flag) =>
        order with { Reservations = order.Reservations.Map(r => r == reservation ? SetReservationEmailFlag(r, flag) : r) };

    static Reservation SetReservationEmailFlag(Reservation reservation, ReservationEmails flag) =>
        reservation with { SentEmails = reservation.SentEmails | flag };

    public static EitherAsync<Failure, OrderId> CreateOrderId(IPersistenceContextFactory contextFactory) =>
        from id in IdGenerator.CreateId(contextFactory, nameof(Order))
        select OrderId.FromInt32(id);
}
