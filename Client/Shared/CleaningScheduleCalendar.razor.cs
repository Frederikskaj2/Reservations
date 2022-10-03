using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class CleaningScheduleCalendar
{
    static readonly LocalDatePattern monthPattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM");
    Dictionary<LocalDate, CleaningScheduleDay>? calendar;
    LocalDate currentMonth;
    string currentMonthValue = "";
    LocalDate firstCalendarDate;
    LocalDate lastCalendarDate;
    List<(LocalDate Date, string Value, string Text)>? months;
    LocalDate? nextMonth;
    OrderingOptions? options;
    List<Resource>? orderedResources;
    LocalDate? previousMonth;

    [Inject] public CultureInfo CultureInfo { get; set; } = null!;
    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public IEnumerable<CleaningTask>? CleaningTasks { get; set; }
    [Parameter] public IReadOnlyDictionary<ResourceId, Resource>? Resources { get; set; }
    [Parameter] public IEnumerable<ReservedDay>? ReservedDays { get; set; }

    protected override async Task OnInitializedAsync() => options = await DataProvider.GetOptionsAsync();

    protected override void OnParametersSet()
    {
        if (CleaningTasks is null)
            return;

        orderedResources = Resources!.Values.OrderBy(resource => resource.Sequence).ToList();
        var pattern = LocalDatePattern.Create("MMMM yyyy", CultureInfo);
        months = GetMonths(DateProvider.Today, CleaningTasks)
            .Select(date => (date, monthPattern.Format(date), pattern.Format(date).Capitalize(CultureInfo)))
            .ToList();
        currentMonth = months[0].Date;
        currentMonthValue = months[0].Value;

        UpdateCalendar(currentMonth);
    }

    void MonthChanged(string value) =>
        UpdateCalendar(monthPattern.Parse(value).Value);

    void UpdateCalendar(LocalDate month)
    {
        var today = DateProvider.Today;
        var firstMonday = month.PreviousMonday();
        var lastSunday = month.PlusMonths(1).PlusDays(-1).PreviousMonday().PlusDays(6);
        var fromDate = today < firstMonday
            ? firstMonday.PlusDays(-options!.AdditionalDaysWhereCleaningCanBeDone)
            : today.PlusDays(-options!.AdditionalDaysWhereCleaningCanBeDone);
        var toDate = lastSunday.PlusDays(1);
        var intervals = CleaningTasks!
            .Where(task => fromDate <= task.End.Date && task.Begin.Date <= toDate)
            .SelectMany(task => task.ToIntervals(), (task, interval) => (Task: task, Interval: interval))
            .GroupBy(tuple => tuple.Interval.Date)
            .ToDictionary(
                grouping => grouping.Key,
                grouping =>
                    grouping.ToDictionary(
                        tuple => tuple.Task.ResourceId,
                        tuple => new CleaningInterval(tuple.Task, tuple.Interval.IsFirstDay, tuple.Interval.IsLastDay)));
        var reservedResourceIds = ReservedDays!
            .Where(day => fromDate <= day.Date && day.Date <= toDate)
            .GroupBy(day => day.Date)
            .ToDictionary(
                grouping => grouping.Key,
                grouping => grouping.Select(day => day.ResourceId).ToHashSet());
        var days = Enumerable.Range(0, Period.Between(firstMonday.PlusDays(-1), lastSunday.PlusDays(1), PeriodUnits.Days).Days + 1)
            .Select(i => firstMonday.PlusDays(i - 1))
            .Select(date => new CleaningScheduleDay(
                date,
                date == today,
                date.Month == month.Month,
                IsHoliday(date),
                intervals.TryGetValue(date, out var intervalsForDay) ? intervalsForDay : CleaningScheduleDay.EmptyIntervals,
                reservedResourceIds.TryGetValue(date, out var reservedResourceIdsForDay)
                    ? reservedResourceIdsForDay
                    : CleaningScheduleDay.EmptyReservedResourceIds))
            .ToList();

        currentMonth = month;
        var currentMonthIndex = months!.FindIndex(tuple => tuple.Date == currentMonth);
        currentMonthValue = months[currentMonthIndex].Value;
        previousMonth = currentMonthIndex > 0 ? months[currentMonthIndex - 1].Date : null;
        nextMonth = currentMonthIndex < months.Count - 1 ? months[currentMonthIndex + 1].Date : null;
        calendar = days.ToDictionary(day => day.Date);
        firstCalendarDate = firstMonday;
        lastCalendarDate = lastSunday;

        StateHasChanged();

        bool IsHoliday(LocalDate d) => d.DayOfWeek is IsoDayOfWeek.Saturday or IsoDayOfWeek.Sunday || DateProvider.Holidays.Contains(d);
    }

    static IEnumerable<LocalDate> GetMonths(LocalDate today, IEnumerable<CleaningTask> cleaningTasks)
    {
        var month = today.MonthStart();
        var endMonth = cleaningTasks.Last().End.Date.MonthStart();
        while (month <= endMonth)
        {
            yield return month;
            month = month.PlusMonths(1);
        }
    }

    static string GetCalendarDayClasses(CleaningScheduleDay day, int dayOfWeek)
    {
        return string.Join(' ', GetClasses());

        IEnumerable<string> GetClasses()
        {
            yield return $"calendar-day-of-week-{dayOfWeek}";
            if (day.IsToday)
                yield return "calendar-today";
            else if (!day.IsCurrentMonth)
                yield return "calendar-other-month";
            if (day.IsHoliday)
                yield return "calendar-holiday";
        }
    }

    static DayType GetDayType(CleaningScheduleDay previousDay, CleaningScheduleDay day, ResourceId resourceId)
    {
        var todayIsReserved = day.ReservedResourceIds.Contains(resourceId);
        var yesterdayIsReserved = previousDay.ReservedResourceIds.Contains(resourceId);
        if (day.Intervals.TryGetValue(resourceId, out var interval))
            return (yesterdayIsReserved, todayIsReserved, interval.IsFirstDay, interval.IsLastDay) switch
            {
                (true, false, true, true) => DayType.ReservationCleaningFree,
                (true, false, true, false) => DayType.ReservationCleaning,
                (true, true, true, true) => DayType.ReservationCleaningReservation,
                (false, true, false, true) => DayType.CleaningReservation,
                (false, false, false, true) => DayType.CleaningFree,
                (false, false, false, false) => DayType.Cleaning,
                _ => throw new InvalidOperationException(
                    $"Unexpected day type condition: {(yesterdayIsReserved, todayIsReserved, interval.IsFirstDay, interval.IsLastDay)} for {day}.")
            };
        if (yesterdayIsReserved && todayIsReserved)
            return DayType.Reservation;
        return todayIsReserved ? DayType.FreeReservation : DayType.Free;
    }

    string? GetTooltip(CleaningScheduleDay day, ResourceId resourceId) =>
        day.Intervals.TryGetValue(resourceId, out var interval)
            ? $"{Resources![resourceId].Name}\r\n🔽 {Formatter.FormatTimeLong(interval.Task.Begin)}\r\n🔼 {Formatter.FormatTimeLong(interval.Task.End)}"
            : null;

    enum DayType
    {
        Free,
        FreeReservation,
        Reservation,
        ReservationCleaningFree,
        ReservationCleaning,
        ReservationCleaningReservation,
        CleaningReservation,
        CleaningFree,
        Cleaning
    }
}
