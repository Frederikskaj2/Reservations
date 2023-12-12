using System.Collections.Generic;
using System.Linq;
using System.Net;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class ReservationValidationFunctions
{
    public static EitherAsync<Failure, Unit> ValidateUserReservations(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, LocalDate today, IEnumerable<Reservation> existingReservations,
        Seq<ReservationModel> reservations) =>
        reservations
            .Map(reservation => ValidateReservation(options, holidays, today, existingReservations, reservation))
            .Traverse(identity)
            .Map(_ => unit)
            .ToAsync();

    public static EitherAsync<Failure, Unit> ValidateUserReservationsWithOwnerPolicies(
        OrderingOptions options, LocalDate today, IEnumerable<Reservation> existingReservations, Seq<ReservationModel> reservations) =>
        reservations
            .Map(reservation => ValidateOwnerReservation(options, today, existingReservations, reservation))
            .Traverse(identity)
            .Map(_ => unit)
            .ToAsync();

    public static EitherAsync<Failure, Unit> ValidateOwnerReservations(
        OrderingOptions options, LocalDate today, IEnumerable<Reservation> existingReservations, Seq<ReservationModel> reservations) =>
        reservations
            .Map(reservation => ValidateOwnerReservation(options, today, existingReservations, reservation))
            .Traverse(identity)
            .Map(_ => unit)
            .ToAsync();

    public static EitherAsync<Failure, Unit> ValidateReservationsCanBeUpdated(Order order) =>
        // TODO: Validate that at least one reservation is updated.
        !order.Flags.HasFlag(OrderFlags.IsOwnerOrder) && !order.Flags.HasFlag(OrderFlags.IsHistoryOrder)
            ? unit
            : Failure.New(HttpStatusCode.Forbidden, $"Reservations cannot be updated on order {order.OrderId}.");

    public static EitherAsync<Failure, Unit> ValidateReservationsCheckingConflicts(
        IEnumerable<Reservation> existingReservations, Seq<ReservationModel> reservations) =>
        reservations.Map(reservation => ValidateNoConflicts(reservation, existingReservations))
            .Traverse(identity)
            .Map(_ => unit)
            .ToAsync();

    static Either<Failure, Unit> ValidateReservation(
        OrderingOptions options,
        IReadOnlySet<LocalDate> holidays,
        LocalDate today,
        IEnumerable<Reservation> existingReservations,
        ReservationModel reservation) =>
        from _1 in ValidateReservationDate(options, today, reservation)
        from _2 in ValidateUserReservationDuration(options, holidays, reservation)
        from _3 in ValidateNoConflicts(reservation, existingReservations)
        select unit;

    static Either<Failure, Unit> ValidateOwnerReservation(
        OrderingOptions options, LocalDate today, IEnumerable<Reservation> existingReservations, ReservationModel reservation) =>
        from _1 in ValidateOwnerReservationDate(options, today, reservation)
        from _2 in ValidateOwnerReservationDuration(options, reservation)
        from _3 in ValidateNoConflicts(reservation, existingReservations)
        select unit;

    static Either<Failure, Unit> ValidateReservationDate(OrderingOptions options, LocalDate today, ReservationModel reservation) =>
        ReservationPolicies.IsReservationDateWithinBounds(options, today, reservation.Extent.Date)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Date of {reservation} is invalid.");

    static Either<Failure, Unit> ValidateUserReservationDuration(OrderingOptions options, IReadOnlySet<LocalDate> holidays, ReservationModel reservation) =>
        ReservationPolicies.IsUserReservationDurationWithinBounds(options, holidays, reservation.Extent, reservation.ResourceType)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Duration of {reservation} is invalid.");

    static Either<Failure, Unit> ValidateOwnerReservationDate(OrderingOptions options, LocalDate today, ReservationModel reservation) =>
        ReservationPolicies.IsOwnerReservationDateWithinBounds(options, today, reservation.Extent.Date)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Date of {reservation} is invalid.");

    static Either<Failure, Unit> ValidateOwnerReservationDuration(OrderingOptions options, ReservationModel reservation) =>
        ReservationPolicies.IsOwnerReservationDurationWithinBounds(options, reservation.Extent, reservation.ResourceType)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Duration of {reservation} is invalid.");

    static Either<Failure, Unit> ValidateNoConflicts(ReservationModel reservation, IEnumerable<Reservation> existingReservations) =>
        existingReservations.Any(r => reservation.ResourceId == r.ResourceId && reservation.Extent.Overlaps(r.Extent))
            ? Failure.New(HttpStatusCode.Conflict, $"{reservation} is in conflict.")
            : unit;
}
