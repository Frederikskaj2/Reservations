using Frederikskaj2.Reservations.Core;
using NodaTime;
using System.Linq;

namespace Frederikskaj2.Reservations.Orders;

static class FinishOwnerOrders
{
    public static FinishOwnerOrdersOutput FinishOwnerOrdersCore(OrderingOptions options, ITimeConverter timeConverter, FinishOwnerOrdersInput input) =>
        new(
            input.Orders
                .Filter(order => ShouldBeHistoryOwnerOrder(options, input.Command.Date, order))
                .Map(order => MakeHistoryOwnerOrder(timeConverter, order)));

    static bool ShouldBeHistoryOwnerOrder(OrderingOptions options, LocalDate date, Order order) =>
        order.Flags.HasFlag(OrderFlags.IsOwnerOrder) &&
        !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) &&
        order.Reservations.All(reservation => ReservationIsCancelledOrOld(options, date, reservation));

    static bool ReservationIsCancelledOrOld(OrderingOptions options, LocalDate date, Reservation reservation) =>
        reservation.Status is ReservationStatus.Cancelled || reservation.Extent.Ends().PlusDays(options.AdditionalDaysWhereCleaningCanBeDone) < date;

    static Order MakeHistoryOwnerOrder(ITimeConverter timeConverter, Order order) =>
        MakeHistoryOwnerOrder(GetLatestReservationEndTimestamp(timeConverter, order), order);

    static Instant GetLatestReservationEndTimestamp(ITimeConverter timeConverter, Order order) =>
        order.Reservations.Map(reservation => GetFinishTimestamp(timeConverter, order, reservation)).OrderDescending().First();

    static Instant GetFinishTimestamp(ITimeConverter timeConverter, Order order, Reservation reservation) =>
        reservation.Status is ReservationStatus.Confirmed
            ? timeConverter.GetMidnight(reservation.Extent.Ends())
            : order.Audits.OrderByDescending(audit => audit.Type is OrderAuditType.CancelReservation).First().Timestamp;

    static Order MakeHistoryOwnerOrder(Instant timestamp, Order order) =>
        order with
        {
            Flags = order.Flags | OrderFlags.IsHistoryOrder,
            Audits = order.Audits.Add(OrderAudit.FinishOrder(timestamp)),
        };
}
