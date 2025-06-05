using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class GetOrders
{
    public static GetOrdersOutput GetOrdersCore(GetOrdersInput input) =>
        new(CreateOrderSummaries(input.Options, input.Today, input.Orders, toHashMap(input.Users.Map(user => (user.UserId, user)))));

    static Seq<OrderSummary> CreateOrderSummaries(OrderingOptions options, LocalDate today, Seq<OrderExcerpt> orders, HashMap<UserId, UserExcerpt> users) =>
        orders.Map(order => CreateSummary(options, today, order, users));

    static OrderSummary CreateSummary(OrderingOptions options, LocalDate today, OrderExcerpt order, HashMap<UserId, UserExcerpt> users) =>
        new(
            order.OrderId,
            order.Flags,
            order.CreatedTimestamp,
            GetNextReservationDate(today, GetSortedReservations(order.Reservations)),
            GetOrderStatus(options, today, order.Reservations),
            order.Description,
            users[order.UserId]);

    static Seq<Reservation> GetSortedReservations(Seq<Reservation> reservations) =>
        reservations
            .Filter(reservation => reservation.Status is ReservationStatus.Reserved or ReservationStatus.Confirmed)
            .OrderBy(reservation => reservation.Extent.Date)
            .ToSeq();

    static LocalDate GetNextReservationDate(LocalDate today, Seq<Reservation> sortedReservations) =>
        sortedReservations.FindSeq(reservation => reservation.Extent.Date >= today).Case switch
        {
            Reservation futureReservation => futureReservation.Extent.Date,
            // Use today in the extreme rare event where all reservations on an
            // owner order are canceled, but the order hasn't been converted to a
            // history order yet.
            _ => !sortedReservations.IsEmpty ? sortedReservations.Last.Extent.Date : today,
        };

    static OrderCategory GetOrderStatus(OrderingOptions options, LocalDate today, Seq<Reservation> reservations) =>
        reservations.Any(reservation => reservation.Status is ReservationStatus.Reserved)
            ? OrderCategory.Reserved
            : GetReservationsToSettle(options, today, reservations).Any()
                ? OrderCategory.NeedsSettlement
                : OrderCategory.Confirmed;

    static Seq<Reservation> GetReservationsToSettle(OrderingOptions options, LocalDate today, Seq<Reservation> reservations) =>
        reservations.Filter(
            reservation =>
                reservation.Status is ReservationStatus.Confirmed &&
                (reservation.Extent.Ends() <= today || (options.Testing?.IsSettlementAlwaysAllowed ?? false)));
}
