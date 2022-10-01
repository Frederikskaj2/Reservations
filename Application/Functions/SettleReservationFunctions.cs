using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Net;
using static Frederikskaj2.Reservations.Application.AccountsReceivableFunctions;
using static Frederikskaj2.Reservations.Application.TransactionFunctions;
using static Frederikskaj2.Reservations.Application.UserBalanceFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class SettleReservationFunctions
{
    public static EitherAsync<Failure, Reservation> GetReservation(
        OrderingOptions options, IDateProvider dateProvider, Instant timestamp, Order order, ReservationId reservationId, Amount damagesAmount) =>
        from reservation in GetReservation(order, reservationId)
        from _1 in ValidateReservationStatus(reservation)
        from _2 in ValidateDamagesAmount(reservation, damagesAmount)
        from _3 in ValidateSettlementDate(options, dateProvider, timestamp, reservation)
        select reservation;

    static EitherAsync<Failure, Reservation> GetReservation(Order order, ReservationId reservationId) =>
        reservationId.OrderId == order.OrderId && 0 <= reservationId.Index && reservationId.Index < order.Reservations.Count
            ? order.Reservations[reservationId.Index.ToInt32()]
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Reservation {reservationId} does not belong to order {order.OrderId}.");

    static EitherAsync<Failure, Unit> ValidateReservationStatus(Reservation reservation) =>
        reservation.Status == ReservationStatus.Confirmed
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Reservation with status {reservation.Status} cannot be settled.");

    static EitherAsync<Failure, Unit> ValidateDamagesAmount(Reservation reservation, Amount damagesAmount) =>
        damagesAmount <= reservation.Price!.Deposit ? unit : Failure.New(HttpStatusCode.UnprocessableEntity, "Damages exceeds deposit.");

    static EitherAsync<Failure, Unit> ValidateSettlementDate(
        OrderingOptions options, IDateProvider dateProvider, Instant timestamp, Reservation reservation) =>
        reservation.Extent.Ends().At(options.CheckOutTime) < dateProvider.GetTime(timestamp) || (options.Testing?.IsSettlementAlwaysAllowed ?? false)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, "Settlement is too early.");

    public static EitherAsync<Failure, IPersistenceContext> SettleReservation(
        SettleReservationCommand command, LocalDate date, IPersistenceContext context, Order order, Reservation reservation) =>
        from transactionId in IdGenerator.CreateId(context.Factory, nameof(Transaction))
        let transaction = CreateSettleTransaction(
            command.Timestamp, command.AdministratorUserId, command.Damages, command.Description, date, order, reservation, transactionId)
        let context1 = UpdateOrderAndUser(command, context, transaction)
        let context2 = ApplyCreditToOrders(command.Timestamp, command.AdministratorUserId, context1, transactionId)
        select AddTransaction(context2, transaction);

    static IPersistenceContext UpdateOrderAndUser(SettleReservationCommand command, IPersistenceContext context, Transaction transaction) =>
        UpdateOrder(command, UpdateUser(context, transaction), transaction.TransactionId);

    static IPersistenceContext UpdateOrder(SettleReservationCommand command, IPersistenceContext context, TransactionId transactionId) =>
        context.UpdateItem<Order>(Order.GetId(command.OrderId), order => UpdateOrder(command, order, transactionId));

    static Order UpdateOrder(SettleReservationCommand command, Order order, TransactionId transactionId) =>
        TryAddLineItemToOrder(command, UpdateReservationStatusAndAddAudit(command, order, transactionId));

    static Order TryAddLineItemToOrder(SettleReservationCommand command, Order order) =>
        command.Description.Case switch
        {
            string value => AddLineItemToOrder(command, value, order),
            _ => order
        };

    static Order AddLineItemToOrder(SettleReservationCommand command, string description, Order order) =>
        order with { User = order.User! with { AdditionalLineItems = order.User.AdditionalLineItems.Add(CreateLineItem(command, description)) } };

    static LineItem CreateLineItem(SettleReservationCommand command, string description) =>
        new(command.Timestamp, LineItemType.Damages, null, new Damages(command.ReservationId.Index, description), -command.Damages);

    static Order UpdateReservationStatusAndAddAudit(SettleReservationCommand command, Order order, TransactionId transactionId) =>
        order with
        {
            Reservations = UpdateReservations(command.ReservationId, order.Reservations),
            Audits = order.Audits.Add(CreateAudit(command, transactionId))
        };

    static Seq<Reservation> UpdateReservations(ReservationId reservationId, Seq<Reservation> reservations) =>
        reservations.Map((index, reservation) => UpdateReservation(reservationId, reservation, index)).ToSeq();

    static Reservation UpdateReservation(ReservationId reservationId, Reservation reservation, ReservationIndex index) =>
        reservationId.Index == index
            ? reservation with { Status = ReservationStatus.Settled }
            : reservation;

    static OrderAudit CreateAudit(SettleReservationCommand command, TransactionId transactionId) =>
        new(command.Timestamp, command.AdministratorUserId, OrderAuditType.SettleReservation, transactionId);

    static IPersistenceContext UpdateUser(IPersistenceContext context, Transaction transaction) =>
        context.UpdateItem<User>(user => UpdateUserBalance(user, transaction));

    static IPersistenceContext AddTransaction(IPersistenceContext context, Transaction transaction) =>
        context.CreateItem(Transaction.GetId(transaction.TransactionId), transaction);
}
