using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class OrdersFunctions
{
    public static EitherAsync<Failure<Unit>, ETaggedEntity<Order>> ReadResidentOrderEntity(
        IEntityReader reader, OrderId orderId, CancellationToken cancellationToken) =>
        from orderEntity in reader.ReadWithETag<Order>(orderId, cancellationToken).MapReadError()
        from _ in ValidateIsResidentOrder(orderEntity).ToAsync()
        select orderEntity;

    static Either<Failure<Unit>, ETaggedEntity<Order>> ValidateIsResidentOrder(ETaggedEntity<Order> orderEntity) =>
        !orderEntity.Value.Flags.HasFlag(OrderFlags.IsOwnerOrder)
            ? orderEntity
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Order {orderEntity.Value.OrderId} is not a resident order.");

    public static EitherAsync<Failure<Unit>, Unit> SendReservationsCancelledEmail(
        IOrdersEmailService emailService,
        HashSet<ReservationIndex> cancelledReservations,
        User user,
        Order order,
        Option<Transaction> transactionOption,
        CancellationToken cancellationToken) =>
        !cancelledReservations.IsEmpty
            ? SendReservationsCancelledEmail(
                emailService,
                order,
                user,
                cancelledReservations.Order().Map(index => order.Reservations[index.ToInt32()]).ToSeq(), GetFee(transactionOption),
                cancellationToken)
            : unit;

    static Amount GetFee(Option<Transaction> transactionOption) =>
        transactionOption.Case switch
        {
            Transaction transaction => transaction.Amounts[Account.CancellationFees],
            _ => Amount.Zero,
        };

    static EitherAsync<Failure<Unit>, Unit> SendReservationsCancelledEmail(
        IOrdersEmailService emailService, Order order, User user, Seq<Reservation> cancelledReservations, Amount fee, CancellationToken cancellationToken) =>
        emailService.Send(
            new ReservationsCancelledEmailModel(
                user.Email(),
                user.FullName,
                order.OrderId,
                cancelledReservations.Map(reservation => new ReservationDescription(reservation.ResourceId, reservation.Extent.Date)),
                GetRefund(cancelledReservations, fee),
                -fee),
            cancellationToken);

    static Amount GetRefund(Seq<Reservation> cancelledReservations, Amount fee) =>
        cancelledReservations
            .Filter(reservation => reservation.Status == ReservationStatus.Cancelled)
            .Sum(reservation => reservation.Price.Case switch
            {
                Price price => price.Total() + fee,
                _ => throw new UnreachableException(),
            });

    public static Seq<Reservation> GetActiveReservations(Seq<ETaggedEntity<Order>> activeOrders) =>
        activeOrders.ToValues()
            .Bind(order => order.Reservations.Filter(reservation => reservation.Status is ReservationStatus.Reserved or ReservationStatus.Confirmed));

    public static Seq<ReservationWithOrder> GetActiveReservationsWithOrders(Seq<ETaggedEntity<Order>> activeOrders) =>
        activeOrders.Bind(
            order => order.Value.Reservations
                .Map((index, reservation) => new ReservationWithOrder(reservation, order.Value, index))
                .Filter(reservationWithOrder => reservationWithOrder.Reservation.Status is ReservationStatus.Reserved or ReservationStatus.Confirmed)
                .ToSeq());

    public static Either<Failure<Unit>, Unit> ValidateNoConflicts(ReservationModel reservation, Seq<Reservation> existingReservations) =>
        existingReservations
            .Any(existingReservation => reservation.ResourceId == existingReservation.ResourceId && reservation.Extent.Overlaps(existingReservation.Extent))
            ? Failure.New(HttpStatusCode.Conflict, $"{reservation} is in conflict.")
            : unit;

    public static EitherAsync<Failure<Unit>, Unit> SendOrderConfirmedEmail(
        IOrdersEmailService emailService, User user, Order order, CancellationToken cancellationToken) =>
        emailService.Send(new OrderConfirmedEmailModel(user.Email(), user.FullName, order.OrderId), cancellationToken);

    public static EitherAsync<Failure<Unit>, HashMap<UserId, string>> ReadUserFullNamesMapForOrder(
        IEntityReader reader, Order order, CancellationToken cancellationToken) =>
        ReadUserFullNamesMap(reader, GetUserIds(order), cancellationToken);

    static HashSet<UserId> GetUserIds(Order order) =>
        toHashSet(order.Audits.Map(audit => audit.UserId).Somes());

    public static Seq<Order> SetReservationsEmailFlag(Seq<ReservationWithOrder> reservations, ReservationEmails flag) =>
        toHashSet(reservations.Map(reservation => SetReservationEmailFlag(reservation.Order, reservation.Reservation, flag))).ToSeq();

    static Order SetReservationEmailFlag(Order order, Reservation reservation, ReservationEmails flag) =>
        order with { Reservations = order.Reservations.Map(r => r == reservation ? SetReservationEmailFlag(r, flag) : r) };

    static Reservation SetReservationEmailFlag(Reservation reservation, ReservationEmails flag) =>
        reservation with { SentEmails = reservation.SentEmails | flag };
}
