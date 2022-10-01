using Frederikskaj2.Reservations.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class CalendarFunctions
{
    public static IEnumerable<Reservation> FilterReservations(
        this IEnumerable<Reservation> reservations, Func<ReservationStatus, bool> statusPredicate, ReservedDaysCommand command) =>
        reservations
            .Filter(reservation => statusPredicate(reservation.Status))
            .Filter(command.FromDate, (day, date) => date <= day.Extent.Date)
            .Filter(command.ToDate, (day, date) => day.Extent.Date <= date);

    public static IEnumerable<ReservedDay> GetReservedDays(IEnumerable<Reservation> reservations) =>
        reservations
            .Filter(reservation => reservation.Status is ReservationStatus.Confirmed or ReservationStatus.Settled)
            .Map(CreateReservedDays)
            .Flatten()
            .OrderBy(day => day.Date)
            .ThenBy(day => day.ResourceId);

    static IEnumerable<ReservedDay> CreateReservedDays(Reservation reservation) =>
        Range(0, reservation.Extent.Nights).Select(i => new ReservedDay(reservation.Extent.Date.PlusDays(i), reservation.ResourceId));


}
