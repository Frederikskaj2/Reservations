using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleaningSchedule = Frederikskaj2.Reservations.Shared.Email.CleaningSchedule;

namespace Frederikskaj2.Reservations.EmailSender;

public static class CleaningTasksFunctions
{
    static readonly IReadOnlyList<Cell> headerCells = new Cell[]
    {
        new HeaderCell("Mandag"),
        new HeaderCell("Tirsdag"),
        new HeaderCell("Onsdag"),
        new HeaderCell("Torsdag"),
        new HeaderCell("Fredag"),
        new HeaderCell("Lørdag"),
        new HeaderCell("Søndag")
    };

    public static async ValueTask<EmailCleaningSchedule> CreateCleaningSchedule(CleaningSchedule cleaningSchedule, CancellationToken cancellationToken)
    {
        // TODO: Handle empty tasks.
        var (firstDay, lastDay) = cleaningSchedule.Schedule.CleaningTasks.Aggregate(
            (Minimum: LocalDate.MaxIsoValue, Maximum: LocalDate.MinIsoValue),
            (tuple, task) => (LocalDate.Min(tuple.Minimum, task.Begin.Date), LocalDate.Max(tuple.Maximum, task.End.Date)));
        var orderedResources = cleaningSchedule.Resources.OrderBy(resource => resource.Sequence);
        var months = await GetMonthTables(cleaningSchedule.Schedule.CleaningTasks,
                cleaningSchedule.Schedule.ReservedDays,
                firstDay,
                lastDay,
                orderedResources.Select(resource => resource.ResourceId).ToArray())
            .ToAsyncEnumerable()
            .SelectAwait(async tuple => new MonthCalendar(
                tuple.Month,
                await CleaningSchedulePictureGenerator.GenerateMonthPicture(tuple.Table, cancellationToken)))
            .ToListAsync(cancellationToken);
        var legend = await CleaningSchedulePictureGenerator.GenerateLegend(orderedResources.Select(resource => resource.Name).ToArray(), cancellationToken);
        return new EmailCleaningSchedule(
            cleaningSchedule.Email,
            cleaningSchedule.FullName,
            cleaningSchedule.Schedule.CleaningTasks,
            cleaningSchedule.Delta.NewTasks,
            cleaningSchedule.Delta.CancelledTasks,
            cleaningSchedule.Delta.UpdatedTasks,
            cleaningSchedule.Resources.ToDictionary(resource => resource.ResourceId, resource => resource.Name),
            months,
            legend);
    }

    static IEnumerable<(LocalDate Month, Table Table)> GetMonthTables(
        IEnumerable<CleaningTask> tasks, IEnumerable<ReservedDay> reservedDays, LocalDate firstDay, LocalDate lastDay,
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
                .Where(task => firstMonday <= task.End.Date && task.Begin.Date <= toDate)
                .SelectMany(task => task.ToIntervals(), (task, interval) => (Task: task, Interval: interval))
                .GroupBy(tuple => tuple.Interval.Date)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping =>
                        grouping.ToDictionary(
                            tuple => tuple.Task.ResourceId,
                            tuple => new CleaningInterval(tuple.Interval.IsFirstDay, tuple.Interval.IsLastDay)));
            if (intervals.Count is 0)
                continue;
            var reservedResourceIds = reservedDays
                .Where(day => fromDate.PlusDays(-1) <= day.Date && day.Date <= toDate)
                .GroupBy(day => day.Date)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping.Select(day => day.ResourceId).ToHashSet());
            var days = Enumerable.Range(0, Period.Between(firstMonday.PlusDays(-1), lastSunday.PlusDays(1), PeriodUnits.Days).Days + 1)
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
            new Row(Design.HeaderCellHeight, headerCells),
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
        LocalDate month, LocalDate firstMonday, LocalDate lastSunday, IReadOnlyDictionary<LocalDate, CleaningScheduleDay> days,
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
                    $"Unexpected day type condition: {(yesterdayIsReserved, todayIsReserved, interval.IsFirstDay, interval.IsLastDay)} for {day}.")
            };
        if (yesterdayIsReserved && todayIsReserved)
            return ResourceUsages.Reservation;
        return todayIsReserved ? ResourceUsages.FreeReservation : ResourceUsages.Free;
    }
}
