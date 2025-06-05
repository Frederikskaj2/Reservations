using Frederikskaj2.Reservations.Client.Dialogs;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.OrderHandling))]
public partial class OwnerCalendarPage
{
    bool isInitialized;
    OrderingOptions? options;
    ReservationDialog reservationDialog = null!;
    IEnumerable<MyReservedDay>? reservedDays;
    IReadOnlyDictionary<ResourceId, Resource>? resources;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public AsyncEventAggregator AsyncEventAggregator { get; set; } = null!;
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
        var response = await ApiClient.GetAsync<IEnumerable<MyReservedDay>>("reserved-days/owner");
        return reservedDays = response.Result ?? Enumerable.Empty<MyReservedDay>();
    }

    bool IsResourceAvailable(IReadOnlySet<LocalDate> reservedDays, LocalDate today, LocalDate date) =>
        ReservationPolicies.IsResourceAvailableToOwner(options!, reservedDays, today, date);

    void MakeReservation((ResourceId, LocalDate) tuple)
    {
        var (resourceId, date) = tuple;
        var resource = resources![resourceId];
        var fromDate = date.PlusDays(-options!.MaximumAllowedReservationNights);
        var toDate = date.PlusDays(options!.MaximumAllowedReservationNights);
        var resourceReservedDays = reservedDays!
            .Where(reservedDay => reservedDay.ResourceId == resource.ResourceId && fromDate <= reservedDay.Date && reservedDay.Date <= toDate)
            .Select(reservedDay => reservedDay.Date)
            .ToList();
        var (minimumDays, maximumDays) =
            ReservationPolicies.GetOwnerAllowedNights(options!, resourceReservedDays, date);
        reservationDialog.Options = options;
        reservationDialog.MinimumNights = minimumDays;
        reservationDialog.MaximumNights = maximumDays;
        DraftOrder.AddReservation(resource, new Extent(date, 0));
        reservationDialog.Show();
    }

    void CancelReservation() => DraftOrder.ClearReservation();

    void ConfirmReservation() => DraftOrder.AddReservationToOrder();

    void Checkout() => NavigationManager.NavigateTo(Urls.OwnerCheckout1);
}
