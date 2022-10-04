using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System.Net;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.EmailFunctions;
using static Frederikskaj2.Reservations.Application.OrderFunctions;
using static Frederikskaj2.Reservations.Application.ReservationValidationFunctions;
using static Frederikskaj2.Reservations.Application.TransactionFunctions;
using static Frederikskaj2.Reservations.Application.UpdateReservationsFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class UpdateUserReservationsHandler
{
    public static EitherAsync<Failure, Unit> Handle(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, OrderingOptions options,
        UpdateUserReservationsCommand command) =>
        from context1 in ReadAllOrdersAndUserFromOrderContext(CreateContext(contextFactory), command.OrderId)
        let order = context1.Order(command.OrderId)
        from _1 in ValidateReservationsCanBeUpdated(order)
        let today = dateProvider.GetDate(command.Timestamp)
        let existingReservations = GetReservationsWithOrders(context1.Items<Order>()).ToSeq()
        let otherExistingReservations = existingReservations.Filter(reservationWithOrder => reservationWithOrder.Order.OrderId != command.OrderId)
        from reservations in GetReservations(command.Reservations, order)
        from _2 in ValidateReservationsCheckingConflicts(otherExistingReservations.Map(reservationWithOrder => reservationWithOrder.Reservation), reservations)
        from transactionId in CreateTransactionId(contextFactory)
        from context2 in UpdateOrder(options, dateProvider.Holidays, command, today, context1, transactionId)
        from _3 in DatabaseFunctions.WriteContext(context2)
        from _4 in SendOrdersConfirmedEmail(emailService, context1, context2)
        select unit;

    static EitherAsync<Failure, Seq<ReservationModel>> GetReservations(Seq<ReservationUpdate> reservations, Order order) =>
        reservations.Map(update => GetReservation(update, order)).TraverseSerial(identity);

    static EitherAsync<Failure, ReservationModel> GetReservation(ReservationUpdate update, Order order) =>
        0 <= update.ReservationIndex && update.ReservationIndex < order.Reservations.Count
            ? GetReservation(order, order.Reservations[update.ReservationIndex.ToInt32()])
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"One or more updated reservations on order {order.OrderId} are invalid.");

    static EitherAsync<Failure, ReservationModel> GetReservation(Order order, Reservation reservation) =>
        Resources.GetResourceType(reservation.ResourceId).Case switch
        {
            ResourceType resourceType => new ReservationModel(reservation.ResourceId, resourceType, reservation.Extent),
            _ => Failure.New(HttpStatusCode.InternalServerError, $"Order {order.OrderId} has a reservation with an invalid resource type.")
        };
}
