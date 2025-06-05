using Frederikskaj2.Reservations.Calendar;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.ReservationPolicies;

namespace Frederikskaj2.Reservations.Client.Modules.Calendar;

[Authorize(Roles = nameof(Roles.OrderHandling))]
partial class OwnerCalendarPage
{
    bool isInitialized;
    OrderingOptions? options;
    ReservationDialog reservationDialog = null!;
    IEnumerable<ReservedDayDto>? reservedDays;
    IReadOnlyDictionary<ResourceId, Resource>? resources;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
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
        var response = await ApiClient.Get<GetReservedDaysResponse>("reserved-days/owner");
        return reservedDays = response.Result?.ReservedDays ?? [];
    }

    bool IsResourceAvailable(IReadOnlySet<LocalDate> days, LocalDate today, LocalDate date) =>
        IsResourceAvailableToOwner(options!, days, today, date);

    Task MakeReservation((ResourceId, LocalDate) tuple)
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
            GetOwnerAllowedNights(options!, resourceReservedDays, date);
        reservationDialog.Options = options;
        reservationDialog.MinimumNights = minimumDays;
        reservationDialog.MaximumNights = maximumDays;
        DraftOrder.AddReservation(resource, new(date, 0));
        return reservationDialog.Show();
    }

    void CancelReservation() => DraftOrder.ClearReservation();

    void ConfirmReservation() => DraftOrder.AddReservationToOrder();

    void Checkout() => NavigationManager.NavigateTo(UrlPath.OwnerCheckout1);
}
