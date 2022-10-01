using Frederikskaj2.Reservations.Client.Dialogs;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

public partial class CalendarPage
{
    bool isInitialized;
    OrderingOptions? options;
    ReservationDialog reservationDialog = null!;
    IEnumerable<MyReservedDay>? reservedDays;
    IReadOnlyDictionary<ResourceId, Resource>? resources;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public TokenAuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public DraftOrder DraftOrder { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        options = await DataProvider.GetOptionsAsync();
        if (options is not null)
        {
            resources = await DataProvider.GetResourcesAsync();
            reservedDays = await GetReservedDaysAsync();
        }
        isInitialized = true;
    }

    async ValueTask<IEnumerable<MyReservedDay>> GetReservedDaysAsync()
    {
        var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var requestUri = state.User.IsAuthenticated() ? "reserved-days/my" : "reserved-days";
        var response = await ApiClient.GetAsync<IEnumerable<MyReservedDay>>(requestUri);
        return response.Result ?? Enumerable.Empty<MyReservedDay>();
    }

    bool IsResourceAvailable(ResourceId resourceId, IReadOnlySet<LocalDate> reservedDays, LocalDate today, LocalDate date) =>
        ReservationPolicies.IsResourceAvailableToUser(options!, DataProvider.Holidays, reservedDays, today, date, resources![resourceId].Type);

    void MakeReservation((ResourceId, LocalDate) tuple)
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
        var adjustedDate =
            ReservationPolicies.GetReservationDateToAllowReservation(DataProvider.Holidays, resourceReservedDays, date, resources![resourceId].Type);
        var (minimumDays, maximumDays) =
            ReservationPolicies.GetUserAllowedNights(options!, DataProvider.Holidays, resourceReservedDays, adjustedDate, resource.Type);
        // If the date was adjusted to previous day ensure that the original date always is included in the extent.
        minimumDays = Math.Max(minimumDays, (date - adjustedDate).Days + 1);
        reservationDialog.Options = options;
        reservationDialog.MinimumNights = minimumDays;
        reservationDialog.MaximumNights = maximumDays;
        reservationDialog.ShowWarning = adjustedDate != date;
        DraftOrder.AddReservation(resource, new Extent(adjustedDate, 0));
        reservationDialog.Show();
    }

    void CancelReservation() => DraftOrder.ClearReservation();

    void ConfirmReservation() => DraftOrder.AddReservationToOrder();

    void Checkout() => NavigationManager.NavigateTo(Urls.Checkout1);
}
