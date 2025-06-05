using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Diagnostics;
using static Frederikskaj2.Reservations.Orders.TransactionAmounts;

namespace Frederikskaj2.Reservations.Orders;

static class UpdateResidentReservations
{
    public static UpdateResidentReservationsOutput UpdateResidentReservationsCore(
        OrderingOptions options,
        IReadOnlySet<LocalDate> holidays,
        ITimeConverter timeConverter,
        UpdateResidentReservationsInput input) =>
        CreateOutput(timeConverter, input, UpdateOrder(input, GetUpdatedReservations(options, holidays, input.Command.Reservations, input.Order)));

    static Seq<UpdatedReservation> GetUpdatedReservations(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, Seq<ReservationUpdate> reservations, Order order) =>
        reservations.Map(update => GetUpdatedReservation(options, holidays, update, order));

    static UpdatedReservation GetUpdatedReservation(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, ReservationUpdate update, Order order) =>
        GetUpdatedReservation(options, holidays, update, order.Reservations[update.ReservationIndex.ToInt32()]);

    static UpdatedReservation GetUpdatedReservation(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, ReservationUpdate update, Reservation reservation) =>
        Resources.GetResourceType(reservation.ResourceId).Case switch
        {
            ResourceType type => new(
                update.ReservationIndex,
                update.Extent,
                reservation.ResourceId,
                Pricing.GetPrice(options, holidays, update.Extent, type)),
            _ => throw new UnreachableException(),
        };

    static Order UpdateOrder(UpdateResidentReservationsInput input, Seq<UpdatedReservation> reservations) =>
        input.Order with
        {
            Reservations = input.Order.Reservations
                .Map((index, reservation) => UpdateReservation(reservations, index, reservation))
                .ToSeq(),
            Audits = input.Order.Audits.Add(OrderAudit.UpdateReservations(input.Command.Timestamp, input.Command.AdministratorId, input.TransactionId)),
        };

    static Reservation UpdateReservation(Seq<UpdatedReservation> reservations, ReservationIndex reservationIndex, Reservation reservationToUpdate) =>
        reservations.Find(reservationUpdate => reservationUpdate.ReservationIndex == reservationIndex).Case switch
        {
            UpdatedReservation reservation => reservationToUpdate with
            {
                Extent = reservation.Extent,
                Price = reservation.Price,
            },
            _ => reservationToUpdate,
        };

    static UpdateResidentReservationsOutput CreateOutput(ITimeConverter timeConverter, UpdateResidentReservationsInput input, Order updatedOrder) =>
        CreateOutput(input, updatedOrder, CreateTransaction(timeConverter, input, updatedOrder));

    static Transaction CreateTransaction(ITimeConverter timeConverter, UpdateResidentReservationsInput input, Order updatedOrder) =>
        CreateUpdateReservationsTransaction(
            input.Command.Timestamp,
            input.Command.AdministratorId,
            timeConverter.GetDate(input.Command.Timestamp),
            input.Order,
            updatedOrder,
            input.TransactionId,
            GetAccountsPayableToSpend(input.User, input.Order, updatedOrder));

    static Amount GetAccountsPayableToSpend(User user, Order oldOrder, Order newOrder) =>
        Amount.Max(-(newOrder.Price().Total() - oldOrder.Price().Total()), user.Accounts[Account.AccountsPayable]);

    static Transaction CreateUpdateReservationsTransaction(
        Instant timestamp,
        UserId administratorId,
        LocalDate date,
        Order originalOrder,
        Order updatedOrder,
        TransactionId transactionId,
        Amount accountsPayableToSpend) =>
        new(
            transactionId,
            date,
            administratorId,
            timestamp,
            Activity.UpdateOrder,
            originalOrder.UserId,
            CreateDescription(updatedOrder),
            UpdateReservations(originalOrder.Price(), updatedOrder.Price(), accountsPayableToSpend));

    static TransactionDescription CreateDescription(Order order) =>
        new ReservationsUpdate(order.OrderId, order.Reservations.Map(reservation => new ReservedDay(reservation.Extent.Date, reservation.ResourceId)));

    static UpdateResidentReservationsOutput CreateOutput(UpdateResidentReservationsInput input, Order updatedOrder, Transaction transaction) =>
        new(
            updatedOrder,
            input.User.AddTransaction(transaction),
            transaction);
}
