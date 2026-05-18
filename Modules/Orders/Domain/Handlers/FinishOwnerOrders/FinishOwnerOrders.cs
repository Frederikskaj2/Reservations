using Frederikskaj2.Reservations.Core;
using NodaTime;
using System.Linq;

namespace Frederikskaj2.Reservations.Orders;

static class FinishOwnerOrders
{
    public static FinishOwnerOrdersOutput FinishOwnerOrdersCore(OrderingOptions options, ITimeConverter timeConverter, FinishOwnerOrdersInput input) =>
        new(
            input.Orders
                .Filter(order => order.Flags.HasFlag(OrderFlags.IsOwnerOrder) && !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) &&
                                 OwnerOrderNeedsUpdate(options, input.Command.Date, order))
                .Map(order => UpdateOwnerOrder(options, timeConverter, input.Command.Date, order)));

    static bool OwnerOrderNeedsUpdate(OrderingOptions options, LocalDate date, Order order) =>
        order.Reservations.Any(reservation => ReservationIsOld(options, date, reservation) && reservation.EntryCode is not null) ||
        order.Reservations.All(reservation => ReservationIsOld(options, date, reservation) || reservation.Status is ReservationStatus.Cancelled);

    static bool ReservationIsOld(OrderingOptions options, LocalDate date, Reservation reservation) =>
        reservation.Extent.Ends().PlusDays(options.AdditionalDaysWhereCleaningCanBeDone) < date;

    static Order UpdateOwnerOrder(OrderingOptions options, ITimeConverter timeConverter, LocalDate date, Order order) =>
        TryMakeHistoryOwnerOrder(options, timeConverter, date, RemoveEntryCodesFromOldReservations(options, date, order));

    static Order RemoveEntryCodesFromOldReservations(OrderingOptions options, LocalDate date, Order order) =>
        order with
        {
            Reservations = order.Reservations.Map(reservation => RemoveEntryCodeFromOldReservation(options, date, reservation)),
        };

    static Reservation RemoveEntryCodeFromOldReservation(OrderingOptions options, LocalDate date, Reservation reservation) =>
        reservation.Extent.Ends().PlusDays(options.AdditionalDaysWhereCleaningCanBeDone) < date
            ? reservation with { EntryCode = null }
            : reservation;

    static Order TryMakeHistoryOwnerOrder(OrderingOptions options, ITimeConverter timeConverter, LocalDate date, Order order) =>
        order.Reservations.All(reservation => ReservationIsOld(options, date, reservation) || reservation.Status is ReservationStatus.Cancelled)
            ? MakeHistoryOwnerOrder(timeConverter, order)
            : order;


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
