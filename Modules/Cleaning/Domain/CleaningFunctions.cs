using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Cleaning;

public static class CleaningFunctions
{
    static readonly CleaningScheduleId currentCleaningScheduleId = CleaningScheduleId.FromString("current");
    static readonly CleaningScheduleId publishedCleaningScheduleId = CleaningScheduleId.FromString("published");

    public static EitherAsync<Failure<Unit>, OptionalEntity<CleaningSchedule>> ReadCurrentCleaningScheduleEntity(
        IEntityReader reader, CancellationToken cancellationToken) =>
        ReadCleaningScheduleEntity(reader, currentCleaningScheduleId, cancellationToken);

    public static EitherAsync<Failure<Unit>, OptionalEntity<CleaningSchedule>> ReadPublishedCleaningScheduleEntity(
        IEntityReader reader, CancellationToken cancellationToken) =>
        ReadCleaningScheduleEntity(reader, publishedCleaningScheduleId, cancellationToken);

    static EitherAsync<Failure<Unit>, OptionalEntity<CleaningSchedule>> ReadCleaningScheduleEntity(
        IEntityReader reader, CleaningScheduleId cleaningScheduleId, CancellationToken cancellationToken) =>
        reader.ReadOptional(cleaningScheduleId, () => new CleaningSchedule(cleaningScheduleId, Empty, Empty), cancellationToken).MapReadError();

    public static CleaningSchedule CreateCleaningSchedule(OrderingOptions options, LocalDate startDate, Seq<Order> orders) =>
        CreateCleaningSchedule(options, currentCleaningScheduleId, startDate, orders);

    static CleaningSchedule CreateCleaningSchedule(OrderingOptions options, CleaningScheduleId id, LocalDate startDate, Seq<Order> orders) =>
        CreateCleaningSchedule(
            options,
            id,
            GetConfirmedReservationsInRange(GetConfirmedReservations(options, startDate, orders), startDate.PlusDays(options.CleaningSchedulePeriodInDays)));

    static Seq<ReservationWithOrder> GetConfirmedReservations(OrderingOptions options, LocalDate startDate, Seq<Order> orders) =>
        orders
            .Bind(order => order.Reservations
                .Map(reservation => new ReservationWithOrder(reservation, order))
                .Filter(reservationWithOrder =>
                    reservationWithOrder.Reservation.Status is ReservationStatus.Confirmed or ReservationStatus.Settled &&
                    reservationWithOrder.Reservation.Extent.Ends().PlusDays(options.AdditionalDaysWhereCleaningCanBeDone) >= startDate));

    static Seq<ReservationWithOrder> GetConfirmedReservationsInRange(Seq<ReservationWithOrder> reservations, LocalDate endDate) =>
        reservations.Filter(reservation => reservation.Reservation.Extent.Ends() <= endDate);

    static CleaningSchedule CreateCleaningSchedule(OrderingOptions options, CleaningScheduleId id, Seq<ReservationWithOrder> reservations) =>
        new(id, GetCleaningTasks(options, CreateCleaningReservations(reservations)), CreateReservedDays(reservations));

    static Seq<CleaningReservation> CreateCleaningReservations(Seq<ReservationWithOrder> reservations) =>
        reservations.Map(CreateCleaningReservation);

    static CleaningReservation CreateCleaningReservation(ReservationWithOrder reservation) =>
        new(reservation.Reservation.ResourceId, reservation.Reservation.Extent, reservation.Order.Flags.HasFlag(OrderFlags.IsCleaningRequired));

    static Seq<ReservedDay> CreateReservedDays(Seq<ReservationWithOrder> reservations) =>
        reservations.Bind(CreateReservedDays).OrderBy(day => day.Date).ThenBy(day => day.ResourceId).ToSeq();

    static Seq<ReservedDay> CreateReservedDays(ReservationWithOrder reservation) =>
        (0, reservation.Reservation.Extent.Nights).ToSeq()
        .Map(i => new ReservedDay(reservation.Reservation.Extent.Date.PlusDays(i), reservation.Reservation.ResourceId));

    static Seq<CleaningTask> GetCleaningTasks(OrderingOptions options, Seq<CleaningReservation> reservations) =>
        GetCleaningTasks(options, GroupReservationsAsPairsByResourceId(reservations)).OrderBy(task => task.Begin).ThenBy(task => task.ResourceId).ToSeq();

    static IEnumerable<IEnumerable<(CleaningReservation Current, Option<CleaningReservation> Next)>> GroupReservationsAsPairsByResourceId(
        IEnumerable<CleaningReservation> reservations) =>
        reservations
            .GroupBy(reservation => reservation.ResourceId)
            .Map(grouping => grouping.OrderBy(reservationWithOrder => reservationWithOrder.Extent.Date).AsPairs());

    static IEnumerable<CleaningTask> GetCleaningTasks(
        OrderingOptions options, IEnumerable<IEnumerable<(CleaningReservation Current, Option<CleaningReservation> Next)>> groups) =>
        groups.Bind(group => GetCleaningTasksForResource(options, group));

    static IEnumerable<CleaningTask> GetCleaningTasksForResource(
        OrderingOptions options, IEnumerable<(CleaningReservation Current, Option<CleaningReservation> Next)> pairs) =>
        pairs.Map(pair => GetCleaningTaskForReservation(options, pair.Current, pair.Next)).Somes();

    static Option<CleaningTask> GetCleaningTaskForReservation(
        OrderingOptions options, CleaningReservation reservation, Option<CleaningReservation> nextReservation) =>
        nextReservation.Case switch
        {
            CleaningReservation next => GetCleaningTaskForReservation(options, reservation, next),
            _ => GetFullCleaningTask(options, reservation),
        };

    static Option<CleaningTask> GetCleaningTaskForReservation(
        OrderingOptions options, CleaningReservation reservation, CleaningReservation nextReservation) =>
        reservation.Extent.Ends().PlusDays(options.AdditionalDaysWhereCleaningCanBeDone) <= nextReservation.Extent.Date
            ? GetFullCleaningTask(options, reservation)
            : GetPartialCleaningTask(options, reservation, nextReservation);

    static Option<CleaningTask> GetFullCleaningTask(OrderingOptions options, CleaningReservation reservation) =>
        reservation.IsCleaningRequired
            ? new CleaningTask(
                reservation.ResourceId,
                reservation.Extent.Ends().At(options.CheckOutTime),
                reservation.Extent.Ends().PlusDays(options.AdditionalDaysWhereCleaningCanBeDone).At(options.CheckInTime))
            : None;

    static Option<CleaningTask> GetPartialCleaningTask(OrderingOptions options, CleaningReservation reservation, CleaningReservation nextReservation) =>
        reservation.IsCleaningRequired
            ? new CleaningTask(
                reservation.ResourceId,
                reservation.Extent.Ends().At(options.CheckOutTime),
                nextReservation.Extent.Date.At(options.CheckInTime))
            : None;
}
