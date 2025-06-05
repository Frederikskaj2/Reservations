using Frederikskaj2.Reservations.Core;
using LanguageExt;
using NodaTime;
using System.Net;
using static Frederikskaj2.Reservations.Orders.ReservationPolicies;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

static class CancelReservationFunctions
{
    public static Either<Failure<Unit>, Unit> ValidateCancelledReservations(HashSet<ReservationIndex> cancelledReservations, Order order) =>
        cancelledReservations.ForAll(index => 0 <= index && index < order.Reservations.Count)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"One or more cancelled reservations on order {order.OrderId} are invalid.");

    public static Either<Failure<Unit>, Unit> ValidateReservationsCanBeCancelled(
        OrderingOptions options, HashSet<ReservationIndex> cancelledReservations, bool alwaysAllowCancellation, Order order, LocalDate date) =>
        cancelledReservations
            .Sequence(index => ValidateReservationCanBeCancelled(options, date, order[index], alwaysAllowCancellation))
            .Map(_ => unit);

    static Either<Failure<Unit>, Unit> ValidateReservationCanBeCancelled(
        OrderingOptions options, LocalDate date, Reservation reservation, bool alwaysAllowCancellation) =>
        CanReservationBeCancelled(options, date, reservation.Status, reservation.Extent, alwaysAllowCancellation)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Reservation {reservation} cannot be cancelled.");
}
