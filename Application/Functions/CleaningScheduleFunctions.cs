using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.CalendarFunctions;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static LanguageExt.Prelude;
using static System.Linq.Enumerable;

namespace Frederikskaj2.Reservations.Application;

static class CleaningScheduleFunctions
{
    const string singletonId = "";

    public static EitherAsync<Failure, CleaningSchedule> GetCleaningSchedule(
        IPersistenceContextFactory contextFactory, OrderingOptions options, LocalDate date) =>
        from reservations in GetCleaningReservations(CreateContext(contextFactory), options, date)
        let cleaningTasks = GetCleaningTasks(reservations)
        let reservedDays = GetReservedDays(reservations.Map(tuple => tuple.Reservation))
        select new CleaningSchedule(cleaningTasks, reservedDays);

    public static EitherAsync<Failure, (CleaningSchedule Schedule, Option<CleaningTasksDelta> Delta)> TryGetCleaningScheduleDelta(
        IPersistenceContextFactory contextFactory, Instant timestamp, OrderingOptions options, LocalDate date) =>
        from context in GetPersistedCleaningSchedule(CreateContext(contextFactory))
        from tuple in GetCleaningScheduleDelta(context, options, date)
        let optionalDelta = GetCleaningTasksDeltaOption(tuple.Delta)
        from _ in UpdateCleaningScheduleIfNeeded(context, timestamp, tuple.Schedule, optionalDelta)
        select (tuple.Schedule, optionalDelta);

    static EitherAsync<Failure, IPersistenceContext> GetPersistedCleaningSchedule(IPersistenceContext context) =>
        MapReadError(context.ReadItem(singletonId, () => new CleaningTasks(default, Empty<CleaningTask>())));

    static EitherAsync<Failure, (CleaningSchedule Schedule, CleaningTasksDelta Delta)> GetCleaningScheduleDelta(
        IPersistenceContext context, OrderingOptions options, LocalDate date) =>
        from reservations in GetCleaningReservations(context, options, date)
        let cleaningTasks = GetCleaningTasks(reservations)
        let delta = GetCleaningTasksDelta(context.Item<CleaningTasks>().Tasks, cleaningTasks)
        let reservedDays = GetReservedDays(reservations.Map(tuple => tuple.Reservation))
        select (new CleaningSchedule(cleaningTasks, reservedDays), delta);

    static EitherAsync<Failure, IEnumerable<ReservationWithOrder>> GetCleaningReservations(
        IPersistenceContext context, OrderingOptions options, LocalDate date) =>
        from orders in ReadOrders(context)
        let confirmedReservations = GetConfirmedReservations(options, orders, date)
        let endDate = date.PlusDays(options.CleaningSchedulePeriodInDays)
        select confirmedReservations.Filter(reservation => reservation.Reservation.Extent.Ends() <= endDate);

    static IEnumerable<ReservationWithOrder> GetConfirmedReservations(OrderingOptions options, IEnumerable<Order> orders, LocalDate date) =>
        orders
            .Bind(order => order.Reservations
                .Map((index, reservation) => new ReservationWithOrder(reservation, order, index))
                .Filter(reservationWithOrder =>
                    reservationWithOrder.Reservation.Status is ReservationStatus.Confirmed or ReservationStatus.Settled &&
                    reservationWithOrder.Reservation.Extent.Ends().PlusDays(options.AdditionalDaysWhereCleaningCanBeDone) >= date));

    static IEnumerable<CleaningTask> GetCleaningTasks(IEnumerable<ReservationWithOrder> reservations) =>
        reservations
            .Filter(tuple => tuple.Reservation.Cleaning is not null)
            .GroupBy(tuple => tuple.Reservation.ResourceId)
            .SelectMany(grouping => GetCleaningTasksForResource(grouping.OrderBy(reservationWithOrder => reservationWithOrder.Reservation.Extent.Date)))
            .OrderBy(reservation => reservation.Begin)
            .ThenBy(reservation => reservation.ResourceId);

    static IEnumerable<CleaningTask> GetCleaningTasksForResource(IEnumerable<ReservationWithOrder> reservations) =>
        reservations.Map(
            reservationWithOrder => new CleaningTask(reservationWithOrder.Reservation.ResourceId,
                reservationWithOrder.Reservation.Cleaning!.Begin,
                reservationWithOrder.Reservation.Cleaning.End));

    static CleaningTasksDelta GetCleaningTasksDelta(IEnumerable<CleaningTask> persistedTasks, IEnumerable<CleaningTask> tasks) =>
        GetCleaningTasksDelta(persistedTasks, tasks, persistedTasks.Map(task => ((task.Begin, task.ResourceId), task)).ToHashMap());

    static Option<CleaningTasksDelta> GetCleaningTasksDeltaOption(CleaningTasksDelta delta) =>
        delta.NewTasks.Any() || delta.CancelledTasks.Any() || delta.UpdatedTasks.Any()
            ? delta
            : None;

    static EitherAsync<Failure, IPersistenceContext> UpdateCleaningScheduleIfNeeded(
        IPersistenceContext context, Instant timestamp, CleaningSchedule schedule, Option<CleaningTasksDelta> optionalDelta) =>
        optionalDelta.Case switch
        {
            CleaningTasksDelta => UpdateCleaningTasks(context, timestamp, schedule),
            _ => RightAsync<Failure, IPersistenceContext>(context)
        };

    static EitherAsync<Failure, IPersistenceContext> UpdateCleaningTasks(IPersistenceContext context, Instant timestamp, CleaningSchedule schedule) =>
        MapWriteError(context.UpdateItem<CleaningTasks>(_ => UpdateCleaningTasks(timestamp, schedule)).Write());

    static CleaningTasks UpdateCleaningTasks(Instant timestamp, CleaningSchedule schedule) =>
        new(timestamp, schedule.CleaningTasks);

    static CleaningTasksDelta GetCleaningTasksDelta(
        IEnumerable<CleaningTask> persistedTasks, IEnumerable<CleaningTask> tasks, HashMap<(LocalDateTime, ResourceId), CleaningTask> persistedTaskMap) =>
        new(
            tasks.Except(persistedTasks, CleaningTaskComparer.Instance).ToList(),
            persistedTasks.Except(tasks, CleaningTaskComparer.Instance).ToList(),
            tasks.Filter(task => persistedTaskMap.Find((task.Begin, task.ResourceId)).Case switch
            {
                CleaningTask matchingTask => matchingTask != task,
                _ => false
            }).ToList());

    public static IPersistenceContext ScheduleCleaning(OrderingOptions options, IPersistenceContext context) =>
        ScheduleCleaning(options, context, GetReservationWithOrders(context.Items<Order>()));

    static IEnumerable<ReservationWithOrder> GetReservationWithOrders(IEnumerable<Order> orders) =>
        orders
            .Bind(order => order.Reservations.Map((index, reservation) => new ReservationWithOrder(reservation, order, index)))
            .Filter(reservationWithOrder => reservationWithOrder.Reservation.Status is ReservationStatus.Confirmed);

    static IPersistenceContext ScheduleCleaning(OrderingOptions options, IPersistenceContext context, IEnumerable<ReservationWithOrder> reservations) =>
        ScheduleCleaningForReservation(
            options,
            context,
            reservations
                .GroupBy(reservationWithOrder => reservationWithOrder.Reservation.ResourceId)
                .Map(grouping => grouping.OrderBy(reservationWithOrder => reservationWithOrder.Reservation.Extent.Date).AsPostfixPairs().ToSeq())
                .ToSeq());

    static IPersistenceContext ScheduleCleaningForReservation(
        OrderingOptions options, IPersistenceContext context, Seq<Seq<(ReservationWithOrder Current, Option<ReservationWithOrder> Next)>> groups) =>
        groups.Fold(context, (currentContext, pairs) => ScheduleCleaningForReservation(options, currentContext, pairs));

    static IPersistenceContext ScheduleCleaningForReservation(
        OrderingOptions options, IPersistenceContext context, Seq<(ReservationWithOrder Current, Option<ReservationWithOrder> Next)> pairs) =>
        pairs.Fold(context, (currentContext, head) => ScheduleCleaningForReservation(options, currentContext, head.Current, head.Next));

    static IPersistenceContext ScheduleCleaningForReservation(
        OrderingOptions options, IPersistenceContext context, ReservationWithOrder reservation, Option<ReservationWithOrder> nextReservation) =>
        nextReservation.Case switch
        {
            ReservationWithOrder next => ScheduleCleaningForReservation(options, context, reservation, next),
            _ => ScheduleFullCleaningPeriodForReservation(options, context, reservation)
        };

    static IPersistenceContext ScheduleCleaningForReservation(
        OrderingOptions options, IPersistenceContext context, ReservationWithOrder reservation, ReservationWithOrder nextReservation) =>
        reservation.Reservation.Extent.Ends().PlusDays(options.AdditionalDaysWhereCleaningCanBeDone) <= nextReservation.Reservation.Extent.Date
            ? ScheduleFullCleaningPeriodForReservation(options, context, reservation)
            : SchedulePartialCleaningPeriodForReservation(options, context, reservation, nextReservation);

    static IPersistenceContext SchedulePartialCleaningPeriodForReservation(
        OrderingOptions options, IPersistenceContext context, ReservationWithOrder reservation, ReservationWithOrder nextReservation) =>
        TryUpdateCleaningPeriod(context, reservation, GetPartialCleaningPeriod(options, reservation, nextReservation.Reservation));

    static IPersistenceContext ScheduleFullCleaningPeriodForReservation(
        OrderingOptions options, IPersistenceContext context, ReservationWithOrder reservation) =>
        TryUpdateCleaningPeriod(context, reservation, GetFullCleaningPeriod(options, reservation));

    static IPersistenceContext TryUpdateCleaningPeriod(IPersistenceContext context, ReservationWithOrder reservation, CleaningPeriod? period) =>
        reservation.Reservation.Cleaning != period
            ? context.UpdateItem<Order>(Order.GetId(reservation.Order.OrderId), order => UpdateReservation(order, reservation, period))
            : context;

    static Order UpdateReservation(Order order, ReservationWithOrder reservationWithOrder, CleaningPeriod? period) =>
        order with { Reservations = UpdateReservations(order.Reservations, reservationWithOrder.Index, period) };

    static Seq<Reservation> UpdateReservations(Seq<Reservation> reservations, int indexToUpdate, CleaningPeriod? period) =>
        reservations.Map((index, reservation) => indexToUpdate == index ? reservation with { Cleaning = period } : reservation).ToSeq();

    static CleaningPeriod? GetFullCleaningPeriod(OrderingOptions options, ReservationWithOrder reservation) =>
        reservation.Order.Flags.HasFlag(OrderFlags.IsCleaningRequired)
            ? new CleaningPeriod(
                reservation.Reservation.Extent.Ends().At(options.CheckOutTime),
                reservation.Reservation.Extent.Ends().PlusDays(options.AdditionalDaysWhereCleaningCanBeDone).At(options.CheckInTime))
            : null;

    static CleaningPeriod? GetPartialCleaningPeriod(OrderingOptions options, ReservationWithOrder reservation, Reservation nextReservation) =>
        reservation.Order.Flags.HasFlag(OrderFlags.IsCleaningRequired)
            ? new CleaningPeriod(
                reservation.Reservation.Extent.Ends().At(options.CheckOutTime),
                nextReservation.Extent.Date.At(options.CheckInTime))
            : null;

    public static EitherAsync<Failure, Unit> SendCleaningScheduleEmail(IEmailService emailService, User user, CleaningSchedule schedule) =>
        emailService
            .Send(
                new CleaningScheduleEmail(
                    schedule,
                    new CleaningTasksDelta(Empty<CleaningTask>(), Empty<CleaningTask>(), Empty<CleaningTask>())),
                new EmailUser { Email = user.Email(), FullName = user.FullName }.Yield())
            .ToRightAsync<Failure, Unit>();
}
