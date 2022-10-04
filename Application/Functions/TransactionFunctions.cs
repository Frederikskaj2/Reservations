using LanguageExt;
using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using static Frederikskaj2.Reservations.Application.TransactionAmounts;
using static Frederikskaj2.Reservations.Application.TransactionDescription;

namespace Frederikskaj2.Reservations.Application;

static class TransactionFunctions
{
    public static EitherAsync<Failure, TransactionId> CreateTransactionId(IPersistenceContextFactory contextFactory) =>
        from id in IdGenerator.CreateId(contextFactory, nameof(Transaction))
        select TransactionId.FromInt32(id);

    public static Transaction CreatePlaceOrderTransaction(LocalDate date, Order order, TransactionId transactionId, Amount accountsPayableToSpend) =>
        new(
            transactionId,
            date,
            order.UserId,
            order.CreatedTimestamp,
            Activity.PlaceOrder,
            order.UserId,
            order.OrderId,
            null,
            PlaceOrder(order.Price(), accountsPayableToSpend));

    public static Transaction CreateCancelReservationTransaction(
        Instant timestamp, UserId userId, LocalDate date, Order order, Seq<Reservation> cancelledReservations, TransactionId transactionId, Amount fee) =>
        new(
            transactionId,
            date,
            userId,
            timestamp,
            Activity.UpdateOrder,
            order.UserId,
            order.OrderId,
            CreateCancellationDescription(cancelledReservations.Map(reservation => new ReservedDay(reservation.Extent.Date, reservation.ResourceId))),
            GetCancelReservationsAmounts(cancelledReservations, fee));

    static AccountAmounts GetCancelReservationsAmounts(Seq<Reservation> cancelledReservations, Amount fee) =>
        cancelledReservations.Any(reservation => reservation.Status is ReservationStatus.Confirmed)
            ? CancelPaidReservation(GetCancelledReservationsPrice(cancelledReservations), fee)
            : CancelUnpaidReservation(GetCancelledReservationsPrice(cancelledReservations), fee);

    static Price GetCancelledReservationsPrice(Seq<Reservation> cancelledReservations) =>
        cancelledReservations.Fold(new Price(), (sum, reservation) => sum + reservation.Price!);

    public static Transaction CreateUpdateReservationsTransaction(
        Instant timestamp, UserId userId, LocalDate date, Order oldOrder, Order newOrder, Seq<UpdatedReservation> reservations, TransactionId transactionId,
        Amount accountsPayableToSpend) =>
        new(
            transactionId,
            date,
            userId,
            timestamp,
            Activity.UpdateOrder,
            oldOrder.UserId,
            oldOrder.OrderId,
            CreateReservationsUpdateDescription(reservations.Map(reservation => new ReservedDay(reservation.Extent.Date, reservation.ResourceId))),
            UpdateReservations(oldOrder.Price(), newOrder.Price(), accountsPayableToSpend));

    public static Transaction CreatePayInTransaction(PayInCommand command, UserId userId, TransactionId transactionId, Amount excessAmount) =>
        new(
            transactionId,
            command.Date,
            command.AdministratorUserId,
            command.Timestamp,
            Activity.PayIn,
            userId,
            null,
            null,
            PayIn(command.Amount, excessAmount));

    public static Transaction CreateSettleTransaction(Instant timestamp, UserId userId, Amount damages, Option<string> description, LocalDate date, Order order,
        Reservation reservation, TransactionId transactionId) =>
        new(
            transactionId,
            date,
            userId,
            timestamp,
            Activity.SettleReservation,
            order.UserId,
            order.OrderId,
            CreateSettlementDescription(new(reservation.Extent.Date, reservation.ResourceId), description.IfNoneUnsafe((string?) null)),
            Settle(reservation.Price!, damages));

    public static Transaction CreatePayOutTransaction(PayOutCommand command, TransactionId transactionId, Amount excessAmount) =>
        new(
            transactionId,
            command.Date,
            command.AdministratorUserId,
            command.Timestamp,
            Activity.PayOut,
            command.UserId,
            null,
            null,
            PayOut(command.Amount, excessAmount));
}
