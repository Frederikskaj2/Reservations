using Frederikskaj2.Reservations.Cleaning;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Emails.CleaningSchedulePictureGenerator;
using static System.Linq.Enumerable;

namespace Frederikskaj2.Reservations.Emails;

public static class CleaningScheduleFunctions
{
    static readonly IReadOnlyList<Cell> headerCells =
    [
        new HeaderCell("Mandag"),
        new HeaderCell("Tirsdag"),
        new HeaderCell("Onsdag"),
        new HeaderCell("Torsdag"),
        new HeaderCell("Fredag"),
        new HeaderCell("Lørdag"),
        new HeaderCell("Søndag"),
    ];

    public static async ValueTask<CleaningCalendarDto> CreateCleaningCalendar(
        CleaningScheduleOverviewDto cleaningScheduleOverview, CancellationToken cancellationToken)
    {
        var (firstDay, lastDay) = cleaningScheduleOverview.Schedule.CleaningTasks.Aggregate(
            (Minimum: LocalDate.MaxIsoValue, Maximum: LocalDate.MinIsoValue),
            (tuple, task) => (LocalDate.Min(tuple.Minimum, task.Begin.Date), LocalDate.Max(tuple.Maximum, task.End.Date)));
        var orderedResources = cleaningScheduleOverview.Resources.OrderBy(resource => resource.Sequence).ToList();
        var months = await GetMonthTables(
                cleaningScheduleOverview.Schedule.CleaningTasks,
                cleaningScheduleOverview.Schedule.ReservedDays,
                firstDay,
                lastDay,
                orderedResources.Select(resource => resource.ResourceId).ToArray())
            .ToAsyncEnumerable()
            .SelectAwait(async tuple => new MonthCalendar(
                tuple.Month,
                await GenerateMonthPicture(tuple.Table, cancellationToken)))
            .ToListAsync(cancellationToken);
        var legend = await GenerateLegend(orderedResources.Select(resource => resource.Name).ToArray(), cancellationToken);
        return new(
            cleaningScheduleOverview.Schedule.CleaningTasks,
            cleaningScheduleOverview.Delta.NewTasks,
            cleaningScheduleOverview.Delta.CancelledTasks,
            cleaningScheduleOverview.Delta.UpdatedTasks,
            cleaningScheduleOverview.Resources.ToDictionary(resource => resource.ResourceId, resource => resource.Name),
            months,
            legend);
    }

    static IEnumerable<(LocalDate Month, Table Table)> GetMonthTables(
        IEnumerable<CleaningTask> tasks,
        IEnumerable<ReservedDay> reservedDays,
        LocalDate firstDay,
        LocalDate lastDay,
        IEnumerable<ResourceId> orderedResourceIds)
    {
        var firstMonth = new LocalDate(firstDay.Year, firstDay.Month, 1);
        var lastMonth = new LocalDate(lastDay.Year, lastDay.Month, 1);
        for (var month = firstMonth; month <= lastMonth; month = month.PlusMonths(1))
        {
            var firstMonday = month.PreviousMonday();
            var lastSunday = month.PlusMonths(1).PlusDays(-1).PreviousMonday().PlusDays(6);
            var fromDate = firstMonday;
            var toDate = lastSunday.PlusDays(1);
            var intervals = tasks
                .Where(task => fromDate <= task.End.Date && task.Begin.Date <= toDate)
                .SelectMany(task => task.ToIntervals(), (task, interval) => (Task: task, Interval: interval))
                .GroupBy(tuple => tuple.Interval.Date)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping =>
                        grouping.ToDictionary(
                            tuple => tuple.Task.ResourceId,
                            tuple => new CleaningInterval(tuple.Task, tuple.Interval.IsFirstDay, tuple.Interval.IsLastDay)));
            if (intervals.Count is 0)
                continue;
            var theDayBeforeFromDate = fromDate.PlusDays(-1);
            var reservedResourceIds = reservedDays
                .Where(day => theDayBeforeFromDate <= day.Date && day.Date <= toDate)
                .GroupBy(day => day.Date)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping.Select(day => day.ResourceId).ToHashSet());
            var days = Range(0, Period.Between(firstMonday.PlusDays(-1), lastSunday.PlusDays(1), PeriodUnits.Days).Days + 1)
                .Select(i => firstMonday.PlusDays(i - 1))
                .Select(date => new CleaningScheduleDay(
                    date,
                    intervals.TryGetValue(date, out var intervalsForDay) ? intervalsForDay : CleaningScheduleDay.EmptyIntervals,
                    reservedResourceIds.TryGetValue(date, out var reservedResourceIdsForDay)
                        ? reservedResourceIdsForDay
                        : CleaningScheduleDay.EmptyReservedResourceIds))
                .ToList();
            yield return (month, CreateTable(month, firstMonday, lastSunday, days, orderedResourceIds));
        }
    }

    static Table CreateTable(
        LocalDate month, LocalDate firstMonday, LocalDate lastSunday, IEnumerable<CleaningScheduleDay> days, IEnumerable<ResourceId> orderedResourceIds) =>
        new(
            new(Design.HeaderCellHeight, headerCells),
            CreateTableCells(
                    month,
                    firstMonday,
                    lastSunday,
                    days.ToDictionary(day => day.Date),
                    orderedResourceIds)
                .Partition(7)
                .Select(cells => new Row(Design.BodyCellHeight, cells))
                .ToList());

    static IEnumerable<Cell> CreateTableCells(
        LocalDate month,
        LocalDate firstMonday,
        LocalDate lastSunday,
        Dictionary<LocalDate, CleaningScheduleDay> days,
        IEnumerable<ResourceId> orderedResourceIds)
    {
        for (var date = firstMonday; date <= lastSunday; date = date.PlusDays(1))
        {
            var previousDay = days[date.PlusDays(-1)];
            var day = days[date];
            var usages = orderedResourceIds.Select(resourceId => GetResourceUsage(previousDay, day, resourceId)).ToArray();
            yield return new BodyCell(date.Day, usages, month.Month != date.Month, date.DayOfWeek);
        }
    }

    static ResourceUsages GetResourceUsage(CleaningScheduleDay previousDay, CleaningScheduleDay day, ResourceId resourceId)
    {
        var todayIsReserved = day.ReservedResourceIds.Contains(resourceId);
        var yesterdayIsReserved = previousDay.ReservedResourceIds.Contains(resourceId);
        if (day.Intervals.TryGetValue(resourceId, out var interval))
            return (yesterdayIsReserved, todayIsReserved, interval.IsFirstDay, interval.IsLastDay) switch
            {
                (true, false, true, true) => ResourceUsages.ReservationCleaningFree,
                (true, false, true, false) => ResourceUsages.ReservationCleaning,
                (true, true, true, true) => ResourceUsages.ReservationCleaningReservation,
                (false, true, false, true) => ResourceUsages.CleaningReservation,
                (false, false, false, true) => ResourceUsages.CleaningFree,
                (false, false, false, false) => ResourceUsages.Cleaning,
                _ => throw new InvalidOperationException(
                    $"Unexpected day type condition: {(yesterdayIsReserved, todayIsReserved, interval.IsFirstDay, interval.IsLastDay)} for {day}."),
            };
        if (yesterdayIsReserved && todayIsReserved)
            return ResourceUsages.Reservation;
        return todayIsReserved ? ResourceUsages.FreeReservation : ResourceUsages.Free;
    }
}
