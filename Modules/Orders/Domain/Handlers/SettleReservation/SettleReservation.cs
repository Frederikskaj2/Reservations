using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Diagnostics;
using System.Net;
using static Frederikskaj2.Reservations.Orders.HistoryOrderFunctions;
using static Frederikskaj2.Reservations.Orders.ResidentBalanceFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class SettleReservation
{
    public static Either<Failure<Unit>, SettleReservationOutput> SettleReservationCore(
        OrderingOptions options, ITimeConverter timeConverter, SettleReservationInput input) =>
        from reservation in GetReservation(options, timeConverter, input.Command, input.Order)
        let transaction = CreateSettleTransaction(timeConverter, input.Command, input.Order, input.TransactionId, reservation)
        let user = UpdateResidentBalance(input.User, transaction)
        let order = UpdateOrder(input.Command, input.Order, input.TransactionId)
        let tuple = TryMakeHistoryOrder(input.Command.Timestamp, user, order)
        select new SettleReservationOutput(tuple.User, tuple.Order, reservation, transaction);

    static Either<Failure<Unit>, Reservation> GetReservation(
        OrderingOptions options, ITimeConverter timeConverter, SettleReservationCommand command, Order order) =>
        from reservation in GetReservation(order, command.ReservationIndex)
        from _1 in ValidateReservationStatus(reservation)
        from _2 in ValidateDamagesAmount(reservation, command.Damages)
        from _3 in ValidateSettlementDate(options, timeConverter, command.Timestamp, reservation)
        select reservation;

    static Either<Failure<Unit>, Reservation> GetReservation(Order order, ReservationIndex reservationIndex) =>
        0 <= reservationIndex && reservationIndex < order.Reservations.Count
            ? order.Reservations[reservationIndex.ToInt32()]
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Reservation {reservationIndex} is outside bounds on order {order.OrderId}.");

    static Either<Failure<Unit>, Unit> ValidateReservationStatus(Reservation reservation) =>
        reservation.Status is ReservationStatus.Confirmed
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Reservation with status {reservation.Status} cannot be settled.");

    static Either<Failure<Unit>, Price> ValidateDamagesAmount(Reservation reservation, Amount damagesAmount) =>
        reservation.Price.Case switch
        {
            Price price => ValidateDamagesAmount(price, damagesAmount),
            _ => Failure.New(HttpStatusCode.UnprocessableEntity, "Reservation has no price."),
        };

    static Either<Failure<Unit>, Price> ValidateDamagesAmount(Price reservationPrice, Amount damagesAmount) =>
        damagesAmount <= reservationPrice.Deposit ? reservationPrice : Failure.New(HttpStatusCode.UnprocessableEntity, "Damages exceeds deposit.");

    static Either<Failure<Unit>, Unit> ValidateSettlementDate(
        OrderingOptions options, ITimeConverter timeConverter, Instant timestamp, Reservation reservation) =>
        reservation.Extent.Ends().At(options.CheckOutTime) < timeConverter.GetTime(timestamp) || (options.Testing?.IsSettlementAlwaysAllowed ?? false)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, "Settlement is too early.");

    static Transaction CreateSettleTransaction(
        ITimeConverter timeConverter, SettleReservationCommand command, Order order, TransactionId transactionId, Reservation reservation) =>
        new(
            transactionId,
            timeConverter.GetDate(command.Timestamp),
            command.AdministratorId,
            command.Timestamp,
            Activity.SettleReservation,
            order.UserId,
            CreateDescription(command, order, reservation),
            TransactionAmounts.Settle(
                reservation.Price.Case switch
                {
                    Price price => price,
                    _ => throw new UnreachableException(),
                },
                command.Damages));

    static TransactionDescription CreateDescription(SettleReservationCommand command, Order order, Reservation reservation) =>
        new Settlement(order.OrderId, new(reservation.Extent.Date, reservation.ResourceId), command.Description);

    static Order UpdateOrder(SettleReservationCommand command, Order order, TransactionId transactionId) =>
        TryAddLineItemToOrder(command, UpdateReservationStatusAndAddAudit(command, order, transactionId));

    static Order TryAddLineItemToOrder(SettleReservationCommand command, Order order) =>
        command.Description.Case switch
        {
            string description => order with { Specifics = AddLineItemToOrder(command, description, order.Specifics.Resident) },
            _ => order,
        };

    static Resident AddLineItemToOrder(SettleReservationCommand command, string description, Resident resident) =>
        resident with { AdditionalLineItems = resident.AdditionalLineItems.Add(CreateLineItem(command, description)) };

    static LineItem CreateLineItem(SettleReservationCommand command, string description) =>
        new(command.Timestamp, LineItemType.Damages, CancellationFee: null, new(command.ReservationIndex, description), -command.Damages);

    static Order UpdateReservationStatusAndAddAudit(SettleReservationCommand command, Order order, TransactionId transactionId) =>
        order with
        {
            Reservations = UpdateReservations(command.ReservationIndex, order.Reservations),
            Audits = order.Audits.Add(OrderAudit.SettleReservation(command.Timestamp, command.AdministratorId, transactionId)),
        };

    static Seq<Reservation> UpdateReservations(ReservationIndex reservationIndex, Seq<Reservation> reservations) =>
        reservations.Map((index, reservation) => UpdateReservation(reservationIndex, reservation, index)).ToSeq();

    static Reservation UpdateReservation(ReservationIndex indexToUpdate, Reservation reservation, ReservationIndex index) =>
        indexToUpdate == index
            ? reservation with { Status = ReservationStatus.Settled }
            : reservation;
}
