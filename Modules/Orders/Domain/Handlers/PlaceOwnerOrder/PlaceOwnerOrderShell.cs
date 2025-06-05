using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Net;
using System.Threading;
using static Frederikskaj2.Reservations.Orders.OrdersFunctions;
using static Frederikskaj2.Reservations.Orders.PlaceOwnerOrder;
using static Frederikskaj2.Reservations.Orders.ReservationPolicies;
using static Frederikskaj2.Reservations.Persistence.IdGenerator;
using static Frederikskaj2.Reservations.Users.UsersFunctions;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class PlaceOwnerOrderShell
{
    public static EitherAsync<Failure<Unit>, OrderDetails> PlaceOwnerOrder(
        OrderingOptions options,
        IEntityReader reader,
        ITimeConverter timeConverter,
        IEntityWriter writer,
        PlaceOwnerOrderCommand command,
        CancellationToken cancellationToken) =>
        from allActiveOrderEntities in reader.QueryWithETag(OrdersQueries.GetAllActiveOrdersQuery, cancellationToken).MapReadError()
        let today = timeConverter.GetDate(command.Timestamp)
        from _1 in ValidateReservations(options, command.Reservations, GetActiveReservations(allActiveOrderEntities), today)
        from userEntity in reader.ReadWithETag<User>(command.UserId, cancellationToken).MapReadError()
        from orderId in CreateId(reader, writer, nameof(Order), cancellationToken)
        let input = new PlaceOwnerOrderInput(command, userEntity.Value, orderId)
        let output = PlaceOwnerOrderCore(input)
        from _2 in writer.Write(
            collector => collector.Add(userEntity),
            tracker => tracker.Update(output.User).Add(output.Order),
            cancellationToken).MapWriteError()
        select new OrderDetails(output.Order, output.User, CreateUserFullNamesMap(userEntity.Value));

    static EitherAsync<Failure<Unit>, Unit> ValidateReservations(
        OrderingOptions options,
        Seq<ReservationModel> reservations,
        Seq<Reservation> existingReservations,
        LocalDate today) =>
        reservations.Map(reservationModel => ValidateReservation(options, today, existingReservations, reservationModel)).Sequence().Map(_ => unit).ToAsync();

    static Either<Failure<Unit>, Unit> ValidateReservation(
        OrderingOptions options, LocalDate today, Seq<Reservation> existingReservations, ReservationModel reservation) =>
        from _1 in ValidateReservationDate(options, today, reservation)
        from _2 in ValidateReservationDuration(options, reservation)
        from _3 in ValidateNoConflicts(reservation, existingReservations)
        select unit;

    static Either<Failure<Unit>, Unit> ValidateReservationDate(OrderingOptions options, LocalDate today, ReservationModel reservation) =>
        IsOwnerReservationDateWithinBounds(options, today, reservation.Extent.Date)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Date of {reservation} is invalid.");

    static Either<Failure<Unit>, Unit> ValidateReservationDuration(OrderingOptions options, ReservationModel reservation) =>
        IsOwnerReservationDurationWithinBounds(options, reservation.Extent, reservation.ResourceType)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Duration of {reservation} is invalid.");
}
