using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.OrdersFunctions;
using static Frederikskaj2.Reservations.Orders.UpdateResidentReservations;
using static Frederikskaj2.Reservations.Persistence.IdGenerator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class UpdateResidentReservationsShell
{
    public static EitherAsync<Failure<Unit>, Unit> UpdateResidentReservations(
        IReadOnlySet<LocalDate> holidays,
        IJobScheduler jobScheduler,
        OrderingOptions options,
        IEntityReader reader,
        ITimeConverter timeConverter,
        IEntityWriter writer,
        UpdateResidentReservationsCommand command,
        CancellationToken cancellationToken) =>
        from allActiveOrderEntities in reader.QueryWithETag(OrdersQueries.GetAllActiveOrdersQuery, cancellationToken).MapReadError()
        from orderEntity in GetOrderEntity(command.OrderId, allActiveOrderEntities).ToAsync()
        from reservations in ValidateReservations(
            command.Reservations, orderEntity.Value, GetOtherActiveReservations(command.OrderId, allActiveOrderEntities)).ToAsync()
        from _1 in ValidateReservationsCanBeUpdated(orderEntity.Value, reservations).ToAsync()
        from userEntity in reader.ReadWithETag<User>(orderEntity.Value.UserId, cancellationToken).MapReadError()
        from transactionId in CreateId(reader, writer, nameof(Transaction), cancellationToken)
        let input = new UpdateResidentReservationsInput(command, orderEntity.Value, userEntity.Value, transactionId)
        let output = UpdateResidentReservationsCore(options, holidays, timeConverter, input)
        from _2 in Write(writer, orderEntity, userEntity, output, cancellationToken)
        from _3 in ConfirmOrders(jobScheduler, output.User).ToRightAsync<Failure<Unit>, Unit>()
        from _4 in UpdateCleaningSchedule(jobScheduler, output.User).ToRightAsync<Failure<Unit>, Unit>()
        select unit;

    static Either<Failure<Unit>, ETaggedEntity<Order>> GetOrderEntity(OrderId orderId, Seq<ETaggedEntity<Order>> allActiveOrderEntities) =>
        allActiveOrderEntities.FindSeq(entity => entity.Value.OrderId == orderId).Case switch
        {
            ETaggedEntity<Order> entity => entity,
            _ => Failure.New(HttpStatusCode.UnprocessableEntity, "Order does not exist."),
        };

    static Seq<Reservation> GetOtherActiveReservations(OrderId orderId, Seq<ETaggedEntity<Order>> allActiveOrderEntities) =>
        GetActiveReservationsWithOrders(allActiveOrderEntities)
            .Filter(reservationWithOrder => reservationWithOrder.Order.OrderId != orderId)
            .Map(reservationWithOrder => reservationWithOrder.Reservation);

    static Either<Failure<Unit>, Seq<ReservationModel>> ValidateReservations(
        Seq<ReservationUpdate> reservations, Order order, Seq<Reservation> existingReservations) =>
        reservations.Map(update => ValidateReservation(update, order, existingReservations)).Sequence();

    static Either<Failure<Unit>, ReservationModel> ValidateReservation(
        ReservationUpdate reservationUpdate, Order order, Seq<Reservation> existingReservations) =>
        from reservation in ValidateReservation(reservationUpdate, order)
        from _1 in ValidateReservationStatus(reservation)
        from reservationModel in ValidateReservationResourceType(reservationUpdate, reservation)
        from _2 in ValidateNoConflicts(reservationModel, existingReservations)
        select reservationModel;

    static Either<Failure<Unit>, Reservation> ValidateReservation(ReservationUpdate update, Order order) =>
        0 <= update.ReservationIndex && update.ReservationIndex < order.Reservations.Count
            ? order.Reservations[update.ReservationIndex.ToInt32()]
            : Failure.New(HttpStatusCode.UnprocessableEntity, "Some updated reservations are invalid.");

    static Either<Failure<Unit>, Reservation> ValidateReservationStatus(Reservation reservation) =>
        reservation.Status is ReservationStatus.Confirmed
            ? reservation
            : Failure.New(HttpStatusCode.UnprocessableEntity, "Some updated reservations are not confirmed.");

    static Either<Failure<Unit>, ReservationModel> ValidateReservationResourceType(ReservationUpdate reservationUpdate, Reservation reservation) =>
        Resources.GetResourceType(reservation.ResourceId).Case switch
        {
            ResourceType resourceType => new ReservationModel(reservation.ResourceId, resourceType, reservationUpdate.Extent),
            _ => Failure.New(HttpStatusCode.InternalServerError, "Some reservations have an invalid resource type."),
        };

    static Either<Failure<Unit>, Unit> ValidateReservationsCanBeUpdated(Order order, Seq<ReservationModel> reservations) =>
        order.Flags.HasFlag(OrderFlags.IsOwnerOrder) || order.Flags.HasFlag(OrderFlags.IsHistoryOrder)
            ? Failure.New(HttpStatusCode.Forbidden, "Reservations can only be updated on active resident orders.")
            : reservations.IsEmpty
                ? Failure.New(HttpStatusCode.Forbidden, "There are no reservations to update.")
                : unit;

    static EitherAsync<Failure<Unit>, Seq<(EntityOperation Operation, ETag ETag)>> Write(
        IEntityWriter writer,
        ETaggedEntity<Order> orderEntity,
        ETaggedEntity<User> userEntity,
        UpdateResidentReservationsOutput output,
        CancellationToken cancellationToken) =>
        writer
            .Write(
                collector => collector.Add(orderEntity).Add(userEntity),
                tracker => tracker.Update(output.Order).Update(output.User).Add(output.Transaction),
                cancellationToken)
            .MapWriteError();

    static Unit ConfirmOrders(IJobScheduler jobScheduler, User user) =>
        user.IsOwedMoney() ? jobScheduler.Queue(JobName.ConfirmOrders) : unit;

    static Unit UpdateCleaningSchedule(IJobScheduler jobScheduler, User user) =>
        // The confirm orders job will queue the update cleaning schedule job, so this
        // is only necessary if that job wasn't queued.
        !user.IsOwedMoney() ? jobScheduler.Queue(JobName.UpdateCleaningSchedule) : unit;
}
