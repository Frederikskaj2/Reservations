using System;
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
        reservations.Map(reservation =>
                ValidateReservation(options, r => ValidateUserReservationDuration(options, holidays, r), today, existingReservations, reservation))
            .Traverse(identity)
            .Map(_ => unit)
            .ToAsync();

    public static EitherAsync<Failure, Unit> ValidateOwnerReservations(
        OrderingOptions options, LocalDate today, IEnumerable<Reservation> existingReservations, Seq<ReservationModel> reservations) =>
        reservations.Map(reservation =>
                ValidateReservation(options, r => ValidateOwnerReservationDuration(options, r), today, existingReservations, reservation))
            .Traverse(identity)
            .Map(_ => unit)
            .ToAsync();

    static Either<Failure, Unit> ValidateReservation(
        OrderingOptions options, Func<ReservationModel, Either<Failure, Unit>> validateReservationDuration, LocalDate today,
        IEnumerable<Reservation> existingReservations, ReservationModel reservation) =>
        from _1 in ValidateReservationDate(options, today, reservation)
        from _2 in validateReservationDuration(reservation)
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

    static Either<Failure, Unit> ValidateOwnerReservationDuration(OrderingOptions options, ReservationModel reservation) =>
        ReservationPolicies.IsOwnerReservationDurationWithinBounds(options, reservation.Extent, reservation.ResourceType)
            ? unit
            : Failure.New(HttpStatusCode.UnprocessableEntity, $"Duration of {reservation} is invalid.");

    static Either<Failure, Unit> ValidateNoConflicts(ReservationModel reservation, IEnumerable<Reservation> existingReservations) =>
        existingReservations.Any(r => reservation.ResourceId == r.ResourceId && reservation.Extent.Overlaps(r.Extent))
            ? Failure.New(HttpStatusCode.Conflict, $"{reservation} is in conflict.")
            : unit;
}
