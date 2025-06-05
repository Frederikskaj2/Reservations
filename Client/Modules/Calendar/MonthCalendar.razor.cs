using Frederikskaj2.Reservations.Calendar;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Calendar;

public sealed partial class MonthCalendar : IDisposable
{
    static readonly LocalDatePattern monthPattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM");

    Dictionary<LocalDate, Day>? calendar;
    LocalDate currentMonth;
    string currentMonthValue = "";
    ResourceId currentResourceId;
    IDisposable? draftOrderUpdatedSubscription;
    LocalDate firstCalendarDate;
    LocalDate lastCalendarDate;
    List<(LocalDate Date, string Value, string Text)>? months;
    LocalDate? nextMonth;
    List<Resource>? orderedResources;
    LocalDate? previousMonth;
    Dictionary<OrderId, string>? tooltips;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public CultureInfo CultureInfo { get; set; } = null!;
    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public DraftOrder DraftOrder { get; set; } = null!;
    [Inject] public EventAggregator EventAggregator { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    [Parameter] public Func<ResourceId, IReadOnlySet<LocalDate>, LocalDate, LocalDate, bool>? IsResourceAvailable { get; set; }
    [Parameter] public string? LegendTheirReservations { get; set; }
    [Parameter] public string? LegendOurReservations { get; set; }
    [Parameter] public bool LinkToOrders { get; set; }
    [Parameter] public EventCallback<(ResourceId, LocalDate)> OnMakeReservation { get; set; }
    [Parameter] public OrderingOptions? Options { get; set; }
    [Parameter] public IEnumerable<ReservedDayDto>? ReservedDays { get; set; }
    [Parameter] public IReadOnlyDictionary<ResourceId, Resource>? Resources { get; set; }

    public void Dispose() => draftOrderUpdatedSubscription?.Dispose();

    protected override async Task OnInitializedAsync()
    {
        if (LinkToOrders)
            await GetTooltips();

        draftOrderUpdatedSubscription = EventAggregator.Subscribe<DraftOrderUpdatedMessage>(_ => OnDraftOrderUpdated());
        orderedResources = Resources!.Values.OrderBy(resource => resource.Sequence).ToList();
        currentResourceId = orderedResources[0].ResourceId;
        var pattern = LocalDatePattern.Create("MMMM yyyy", CultureInfo);
        months = GetMonths()
            .Select(date => (date, monthPattern.Format(date), pattern.Format(date).Capitalize(CultureInfo)))
            .ToList();
        UpdateCalendar(months![0].Date, orderedResources![0].ResourceId);

        async Task GetTooltips()
        {
            var query = string.Join(
                '&',
                (ReservedDays?.Select(day => day.OrderId).Where(orderId => orderId is not null).Distinct() ?? []).Select(orderId => $"id={orderId}"));
            var response = await ApiClient.Get<GetOrdersResponse>($"orders?{query}");
            if (response.IsSuccess)
            {
                var orders = response.Result!.Orders;
                tooltips = orders.ToDictionary(order => order.OrderId, order => $"Bestilling {order.OrderId}: {order.User.FullName}");
            }
            else
                tooltips = new();
        }
    }

    void OnDraftOrderUpdated() => UpdateCalendar(currentMonth, currentResourceId);

    void MonthChanged(string value) =>
        UpdateCalendar(monthPattern.Parse(value).Value, currentResourceId);

    void SelectMonth(LocalDate month) =>
        UpdateCalendar(month, currentResourceId);

    void SelectResource(ResourceId resourceId) =>
        UpdateCalendar(currentMonth, resourceId);

    void UpdateCalendar(LocalDate month, ResourceId resourceId)
    {
        var reservedDaysIncludingDraftOrder = ReservedDays!.Concat(DraftOrder.ReservedDays());
        if (DraftOrder.DraftReservation is not null)
            reservedDaysIncludingDraftOrder = reservedDaysIncludingDraftOrder.Concat(DraftOrder.DraftReservation.ReservedDays());

        var today = DateProvider.Today;
        var firstMonday = month.PreviousMonday();
        var lastSunday = month.PlusMonths(1).PlusDays(-1).PreviousMonday().PlusDays(6);

        var fromDate = firstMonday.PlusDays(-1);
        var toDate = lastSunday.PlusDays(1);
        var reservedDaysThisMonth = reservedDaysIncludingDraftOrder
            .Where(reservedDay => fromDate <= reservedDay.Date && reservedDay.Date <= toDate)
            .ToList();
        var resourceUsage = reservedDaysThisMonth
            .GroupBy(day => day.Date)
            .ToDictionary(
                grouping => grouping.Key,
                grouping => grouping.ToDictionary(reservation => reservation.ResourceId, reservation => (reservation.IsMyReservation, reservation.OrderId)));
        var resourceReservedDays = reservedDaysThisMonth
            .Where(reservedDay => reservedDay.ResourceId == resourceId)
            .Select(reservedDay => reservedDay.Date)
            .ToHashSet();

        var startDay = firstMonday.PlusDays(-1);
        var dayCount = Period.Between(startDay, lastSunday.PlusDays(1), PeriodUnits.Days).Days + 1;
        var days = Enumerable.Range(0, dayCount)
            .Select(i => startDay.PlusDays(i))
            .Select(date =>
                new Day(date,
                    date == today,
                    date.Month == month.Month,
                    HighPricePolicy.IsHighPriceDay(date, DateProvider.Holidays),
                    IsResourceAvailable!(resourceId, resourceReservedDays, today, date),
                    resourceUsage.TryGetValue(date, out var reservedResources) ? reservedResources : Day.EmptyReservedResources))
            .ToList();

        currentMonth = month;
        var currentMonthIndex = months!.FindIndex(tuple => tuple.Date == currentMonth);
        currentMonthValue = months[currentMonthIndex].Value;
        previousMonth = currentMonthIndex > 0 ? months[currentMonthIndex - 1].Date : null;
        nextMonth = currentMonthIndex < months.Count - 1 ? months[currentMonthIndex + 1].Date : null;
        currentResourceId = resourceId;
        calendar = days.ToDictionary(day => day.Date);
        firstCalendarDate = firstMonday;
        lastCalendarDate = lastSunday;

        StateHasChanged();
    }

    static string GetCalendarDayClasses(Day day, int dayOfWeek)
    {
        return string.Join(' ', GetClasses());

        IEnumerable<string> GetClasses()
        {
            yield return $"calendar-day-of-week-{dayOfWeek}";
            if (day.IsToday)
                yield return "calendar-today";
            else if (!day.IsCurrentMonth)
                yield return "calendar-other-month";
            if (day.IsHighPriceDay)
                yield return "calendar-high-price";
            if (day.IsResourceAvailable)
                yield return "resource-available";
            else
                yield return "resource-unavailable";
        }
    }

    string GetResourceClasses(Day previousDay, Day day, Day nextDay, ResourceId resourceId, int resourceNumber)
    {
        return string.Join(' ', GetClasses());

        IEnumerable<string> GetClasses()
        {
            yield return "resource";
            if (!day.ReservedResources.TryGetValue(resourceId, out var tuple))
                yield break;
            var (isMyReservation, _) = tuple;
            if (isMyReservation)
                yield return "resource-reserved-me";
            else
                yield return "resource-reserved-other";
            yield return $"resource-{resourceNumber}";
            var hasAdjacentReservationBefore = previousDay.ReservedResources.ContainsKey(resourceId);
            if (hasAdjacentReservationBefore)
                yield return "resource-extend-left";
            var hasAdjacentReservationAfter = nextDay.ReservedResources.ContainsKey(resourceId);
            if (hasAdjacentReservationAfter)
                yield return "resource-extend-right";
            var isSelectedResource = resourceId == currentResourceId;
            if (!isSelectedResource)
                yield return "resource-not-current";
            if (LinkToOrders)
                yield return "resource-link-order";
        }
    }

    Task MakeReservation(LocalDate date) => OnMakeReservation.InvokeAsync((currentResourceId, date));

    Task KeyUp(KeyboardEventArgs e, LocalDate date) =>
        e.Key is "Enter" ? OnMakeReservation.InvokeAsync((currentResourceId, date)) : Task.CompletedTask;

    void NavigateToOrder(OrderId orderId) =>
        NavigationManager.NavigateTo($"{UrlPath.Orders}/{orderId}");

    string? GetTooltip(OrderId orderId) =>
        tooltips!.GetValueOrDefault(orderId);

    IEnumerable<LocalDate> GetMonths()
    {
        var today = DateProvider.Today;
        var month = today.MonthStart();
        var endMonth = today.PlusDays(Options!.ReservationIsNotAllowedAfterDaysFromNow).MonthStart();
        while (month <= endMonth)
        {
            yield return month;
            month = month.PlusMonths(1);
        }
    }
}
