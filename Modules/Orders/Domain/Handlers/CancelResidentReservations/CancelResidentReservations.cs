using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Diagnostics;
using System.Linq;
using static Frederikskaj2.Reservations.Orders.CancelReservationFunctions;
using static Frederikskaj2.Reservations.Orders.HistoryOrderFunctions;
using static Frederikskaj2.Reservations.Orders.ResidentBalanceFunctions;
using static Frederikskaj2.Reservations.Orders.TransactionAmounts;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class CancelResidentReservations
{
    public static Either<Failure<Unit>, CancelResidentReservationsOutput> CancelResidentReservationsCore(
        OrderingOptions options, ITimeConverter timeConverter, CancelResidentReservationsInput input) =>
        input.TransactionIdOption.Case switch
        {
            TransactionId transactionId => CancelResidentReservationsCore(options, timeConverter, input, transactionId),
            _ => new CancelResidentReservationsOutput(input.User, input.Order, None),
        };

    static Either<Failure<Unit>, CancelResidentReservationsOutput> CancelResidentReservationsCore(
        OrderingOptions options, ITimeConverter timeConverter, CancelResidentReservationsInput input, TransactionId transactionId) =>
        from _1 in ValidateCancelledReservations(input.CancelledReservations, input.Order)
        let date = timeConverter.GetDate(input.Timestamp)
        from _2 in ValidateReservationsCanBeCancelled(options, input.CancelledReservations, input.AlwaysAllowCancellation, input.Order, date)
        let fee = GetFee(options, input.WaiveFee, input.Order)
        let cancelledReservations = input.CancelledReservations.Order().Map(index => input.Order.Reservations[index.ToInt32()]).ToSeq()
        let transaction = CreateTransaction(input.Timestamp, input.AdministratorId, cancelledReservations, input.Order, transactionId, date, fee)
        let user = UpdateResidentBalance(input.User, transaction)
        let order = UpdateOrder(input.Timestamp, input.AdministratorId, input.CancelledReservations, input.Order, transactionId, fee)
        let tuple = TryMakeHistoryOrder(input.Timestamp, user, order)
        select new CancelResidentReservationsOutput(tuple.User, tuple.Order, transaction);

    static Amount GetFee(OrderingOptions options, bool waiveFee, Order order) =>
        waiveFee || order.NeedsConfirmation() ? Amount.Zero : options.CancellationFee;

    static Transaction CreateTransaction(
        Instant timestamp,
        UserId administratorId,
        Seq<Reservation> cancelledReservations,
        Order order,
        TransactionId transactionId,
        LocalDate date,
        Amount fee) =>
        new(
            transactionId,
            date,
            administratorId,
            timestamp,
            Activity.UpdateOrder,
            order.UserId,
            CreateDescription(order.OrderId, cancelledReservations),
            GetCancelReservationsAmounts(cancelledReservations, fee));

    static TransactionDescription CreateDescription(OrderId orderId, Seq<Reservation> cancelledReservations) =>
        new(new Cancellation(orderId, cancelledReservations.Map(reservation => new ReservedDay(reservation.Extent.Date, reservation.ResourceId))));

    static AccountAmounts GetCancelReservationsAmounts(Seq<Reservation> cancelledReservations, Amount fee) =>
        cancelledReservations.Any(reservation => reservation.Status is ReservationStatus.Confirmed)
            ? CancelPaidReservation(GetCancelledReservationsPrice(cancelledReservations), fee)
            : CancelUnpaidReservation(GetCancelledReservationsPrice(cancelledReservations), fee);

    static Price GetCancelledReservationsPrice(Seq<Reservation> cancelledReservations) =>
        cancelledReservations.Fold(
            new Price(),
            (sum, reservation) => reservation.Price.Case switch
            {
                Price price => sum + price,
                _ => throw new UnreachableException(),
            });

    static Order UpdateOrder(
        Instant timestamp, UserId administratorId, HashSet<ReservationIndex> cancelledReservations, Order order, TransactionId transactionId, Amount fee) =>
        TryAddLineItem(timestamp, cancelledReservations, UpdateOrder(timestamp, administratorId, cancelledReservations, order, transactionId), fee);

    static Order UpdateOrder(
        Instant timestamp, UserId administratorId, HashSet<ReservationIndex> cancelledReservations, Order order, TransactionId transactionId) =>
        order with
        {
            Reservations = order.Reservations.Map((index, reservation) => UpdateReservation(cancelledReservations, reservation, index)).ToSeq(),
            Audits = order.Audits.Add(OrderAudit.CancelResidentReservation(timestamp, administratorId, transactionId)),
        };

    static Reservation UpdateReservation(HashSet<ReservationIndex> cancelledReservations, Reservation reservation, int indexToUpdate) =>
        cancelledReservations.Find(index => index == indexToUpdate).Case switch
        {
            ReservationIndex => reservation with
            {
                Status = reservation.Status is ReservationStatus.Confirmed ? ReservationStatus.Cancelled : ReservationStatus.Abandoned,
            },
            _ => reservation,
        };

    static Order TryAddLineItem(Instant timestamp, HashSet<ReservationIndex> cancelledReservations, Order order, Amount fee) =>
        fee > Amount.Zero
            ? order with { Specifics = AddLineItem(timestamp, cancelledReservations, fee, order.Specifics.Resident) }
            : order;

    static Resident AddLineItem(Instant timestamp, HashSet<ReservationIndex> cancelledReservations, Amount fee, Resident resident) =>
        resident with { AdditionalLineItems = resident.AdditionalLineItems.Add(CreateLineItem(timestamp, cancelledReservations, fee)) };

    static LineItem CreateLineItem(Instant timestamp, HashSet<ReservationIndex> cancelledReservations, Amount fee) =>
        new(timestamp, LineItemType.CancellationFee, new(cancelledReservations), Damages: null, -fee);
}
