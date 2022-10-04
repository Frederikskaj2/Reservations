using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Net;
using static Frederikskaj2.Reservations.Application.AccountsReceivableFunctions;
using static Frederikskaj2.Reservations.Application.CleaningScheduleFunctions;
using static Frederikskaj2.Reservations.Application.TransactionFunctions;

namespace Frederikskaj2.Reservations.Application;

static class UpdateReservationsFunctions
{
    public static EitherAsync<Failure, IPersistenceContext> UpdateOrder(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, UpdateUserReservationsCommand command, LocalDate today, IPersistenceContext context,
        TransactionId transactionId) =>
        UpdateOrder(options, holidays, command, today, context, transactionId, context.Order(command.OrderId));

    static EitherAsync<Failure, IPersistenceContext> UpdateOrder(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, UpdateUserReservationsCommand command, LocalDate today, IPersistenceContext context,
        TransactionId transactionId, Order order) =>
        from updatedReservations in GetUpdatedReservations(options, holidays, command.Reservations, order).ToAsync()
        let newOrder = UpdateOrder(command.Timestamp, command.AdministratorUserId, updatedReservations, order)
        select UpdateOrder(options, command, today, context, transactionId, order, newOrder, updatedReservations);

    static Either<Failure, Seq<UpdatedReservation>> GetUpdatedReservations(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, Seq<ReservationUpdate> reservations, Order order) =>
        reservations.Map(update => GetUpdatedReservationSafe(options, holidays, update, order)).Traverse(Prelude.identity);

    static Either<Failure, UpdatedReservation> GetUpdatedReservationSafe(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, ReservationUpdate update, Order order) =>
        0 <= update.ReservationIndex && update.ReservationIndex < order.Reservations.Count
            ? GetUpdatedReservation(options, holidays, update, order)
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"One or more updated reservations on order {order.OrderId} are invalid.");

    static Either<Failure, UpdatedReservation> GetUpdatedReservation(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, ReservationUpdate update, Order order) =>
        GetUpdatedReservation(options, holidays, update, order, order.Reservations[update.ReservationIndex.ToInt32()]);

    static Either<Failure, UpdatedReservation> GetUpdatedReservation(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, ReservationUpdate update, Order order, Reservation reservation) =>
        Resources.GetResourceType(reservation.ResourceId).Case switch
        {
            ResourceType type => new UpdatedReservation(
                update.ReservationIndex,
                update.Extent,
                reservation.ResourceId,
                Pricing.GetPrice(options, holidays, update.Extent, type)),
            _ => Failure.New(HttpStatusCode.InternalServerError, $"Order {order.OrderId} has a reservation with an invalid resource type.")
        };

    static Order UpdateOrder(Instant timestamp, UserId updatedByUserId, Seq<UpdatedReservation> reservations, Order order) =>
        order with
        {
            Reservations = order.Reservations
                .Map((index, reservation) => UpdateReservation(reservations, index, reservation))
                .ToSeq(),
            Audits = order.Audits.Add(new(timestamp, updatedByUserId, OrderAuditType.UpdateReservations))
        };

    static Reservation UpdateReservation(Seq<UpdatedReservation> reservations, ReservationIndex reservationIndex, Reservation reservationToUpdate) =>
        reservations.Find(reservationUpdate => reservationUpdate.ReservationIndex == reservationIndex).Case switch
        {
            UpdatedReservation reservation => reservationToUpdate with
            {
                Extent = reservation.Extent,
                Price = reservation.Price
            },
            _ => reservationToUpdate
        };

    static IPersistenceContext UpdateOrder(
        OrderingOptions options, UpdateUserReservationsCommand command, LocalDate today, IPersistenceContext context, TransactionId transactionId,
        Order oldOrder, Order newOrder, Seq<UpdatedReservation> reservations) =>
        ScheduleCleaning(
            options,
            ApplyCreditToOrders(
                command.Timestamp,
                command.AdministratorUserId,
                AddUserTransaction(
                    context.UpdateItem<Order>(Order.GetId(command.OrderId), _ => newOrder),
                    CreateUpdateReservationsTransaction(
                        command.Timestamp,
                        command.AdministratorUserId,
                        today,
                        oldOrder,
                        newOrder,
                        reservations,
                        transactionId,
                        GetAccountsPayableToSpend(context, oldOrder, newOrder))),
                transactionId));

    static Amount GetAccountsPayableToSpend(IPersistenceContext context, Order oldOrder, Order newOrder) =>
        Amount.Max(-(newOrder.Price().Total() - oldOrder.Price().Total()), context.Item<User>().Accounts[Account.AccountsPayable]);
}
