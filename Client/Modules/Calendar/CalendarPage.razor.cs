using Frederikskaj2.Reservations.Calendar;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.ReservationPolicies;

namespace Frederikskaj2.Reservations.Client.Modules.Calendar;

partial class CalendarPage
{
    bool isInitialized;
    OrderingOptions? options;
    ReservationDialog reservationDialog = null!;
    IEnumerable<ReservedDayDto>? reservedDays;
    IReadOnlyDictionary<ResourceId, Resource>? resources;

    [Inject] public ApiClient AnonymousApiClient { get; set; } = null!;
    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public TokenAuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public DraftOrder DraftOrder { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        options = await DataProvider.GetOptions();
        if (options is not null)
        {
            resources = await DataProvider.GetResources();
            reservedDays = await GetReservedDays();
        }
        isInitialized = true;
    }

    async ValueTask<IEnumerable<ReservedDayDto>> GetReservedDays()
    {
        var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var response = state.User.IsAuthenticated()
            ? await ApiClient.Get<GetReservedDaysResponse>("reserved-days/my")
            : await AnonymousApiClient.Get<GetReservedDaysResponse>("reserved-days");
        return response.Result?.ReservedDays ?? [];
    }

    bool IsResourceAvailable(ResourceId resourceId, IReadOnlySet<LocalDate> days, LocalDate today, LocalDate date) =>
        IsResourceAvailableToResident(options!, DateProvider.Holidays, days, today, date, resources![resourceId].Type);

    Task MakeReservation((ResourceId, LocalDate) tuple)
    {
        var (resourceId, date) = tuple;
        var resource = resources![resourceId];
        var fromDate = date.PlusDays(-options!.MaximumAllowedReservationNights);
        var toDate = date.PlusDays(options!.MaximumAllowedReservationNights);
        var resourceReservedDays = reservedDays!
            .Concat(DraftOrder.ReservedDays())
            .Where(reservedDay => reservedDay.ResourceId == resource.ResourceId && fromDate <= reservedDay.Date && reservedDay.Date <= toDate)
            .Select(reservedDay => reservedDay.Date)
            .ToList();
        // Automatically adjust the date to a previous date to allow the user to reserve despite a minimum duration of more than one night.
        var adjustedDate = GetReservationDateToAllowReservation(DateProvider.Holidays, resourceReservedDays, date, resources![resourceId].Type);
        var (minimumDays, maximumDays) = GetResidentAllowedNights(options!, DateProvider.Holidays, resourceReservedDays, adjustedDate, resource.Type);
        // If the date was adjusted to previous day ensure that the original date always is included in the extent.
        minimumDays = Math.Max(minimumDays, (date - adjustedDate).Days + 1);
        reservationDialog.Options = options;
        reservationDialog.MinimumNights = minimumDays;
        reservationDialog.MaximumNights = maximumDays;
        reservationDialog.ShowWarning = adjustedDate != date;
        DraftOrder.AddReservation(resource, new(adjustedDate, 0));
        return reservationDialog.Show();
    }

    void CancelReservation() => DraftOrder.ClearReservation();

    void ConfirmReservation() => DraftOrder.AddReservationToOrder();

    void Checkout() => NavigationManager.NavigateTo(UrlPath.Checkout1);
}
