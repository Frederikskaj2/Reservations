using Blazorise;
using Frederikskaj2.Reservations.Calendar;
using Frederikskaj2.Reservations.Client.Modules.Core;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

[Authorize(Roles = nameof(Roles.Resident))]
partial class Checkout2Page
{
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    bool isInitialized;
    bool linkBanquetFacilitiesRules;
    bool linkBedroomRules;
    OrderingOptions? options;
    IndefiniteProgressBar progressBar = null!;
    bool showErrorAlert;
    Amount totalDeposit;
    Amount totalRentAndCleaning;
    Validations validations = null!;

    [Inject] public AuthenticatedApiClient ApiClient { get; init; } = null!;
    [Inject] public ClientDataProvider DataProvider { get; init; } = null!;
    [Inject] public IDateProvider DateProvider { get; init; } = null!;
    [Inject] public DraftOrder DraftOrder { get; init; } = null!;
    [Inject] public Formatter Formatter { get; init; } = null!;
    [Inject] public NavigationManager NavigationManager { get; init; } = null!;
    [Inject] public UserOrderInformation OrderInformation { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        options = await DataProvider.GetOptions();
        apartments = await DataProvider.GetApartments();
        linkBanquetFacilitiesRules = DraftOrder.Reservations.Any(reservation => reservation.Resource.Type == ResourceType.BanquetFacilities);
        linkBedroomRules = DraftOrder.Reservations.Any(reservation => reservation.Resource.Type == ResourceType.Bedroom);
        isInitialized = true;
    }

    protected override void OnParametersSet() =>
        (totalRentAndCleaning, totalDeposit) = DraftOrder.Reservations
            .Select(GetPrice)
            .Aggregate(
                (RentAndCleaning: Amount.Zero, Deposit: Amount.Zero),
                (tuple, price) => (tuple.RentAndCleaning + price.Rent + price.Cleaning, tuple.Deposit + price.Deposit));

    async Task Submit()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        DismissErrorAlert();

        var request = new PlaceMyOrderRequest(FullName: OrderInformation.FullName, Phone: OrderInformation.Phone, ApartmentId: OrderInformation.ApartmentId,
            Reservations: DraftOrder.GetReservationRequests().ToArray(), AccountNumber: OrderInformation.AccountNumber);
        progressBar.Start();
        var placeMyOrderResponse = await ApiClient.Post<PlaceMyOrderResponse>("orders/my", request);
        progressBar.Stop();
        if (placeMyOrderResponse.IsSuccess)
        {
            DataProvider.CurrentOrder = placeMyOrderResponse.Result!.Order;
            DraftOrder.Clear();
            NavigationManager.NavigateTo(UrlPath.Checkout3);
        }
        else if (placeMyOrderResponse.Problem!.Status is HttpStatusCode.Conflict)
        {
            var (fromDate, toDate) = DraftOrder.Reservations.Aggregate(
                (LocalDate.MaxIsoValue, LocalDate.MinIsoValue),
                (tuple, reservation) =>
                {
                    var (from, to) = tuple;
                    return (
                        from < reservation.Extent.Date ? from : reservation.Extent.Date,
                        to > reservation.Extent.Ends() ? to : reservation.Extent.Ends());
                });
            fromDate = fromDate.PlusDays(-options!.MaximumAllowedReservationNights);
            var getReservedDaysResponse = await ApiClient.Get<GetReservedDaysResponse>("reserved-days/my");
            var reservedDays = getReservedDaysResponse.Result!.ReservedDays
                .Where(reservedDay => fromDate <= reservedDay.Date && reservedDay.Date <= toDate)
                .ToList();
            var conflictedReservations = DraftOrder.Reservations.Where(reservation => IsConflicted(reservedDays, reservation));
            foreach (var reservation in conflictedReservations.ToList())
                DraftOrder.RemoveReservation(reservation);
            NavigationManager.NavigateTo(UrlPath.CheckoutConflict);
        }

        showErrorAlert = true;
    }

    static bool IsConflicted(List<ReservedDayDto> reservedDays, DraftReservation reservation) =>
        reservedDays.Exists(day => reservation.Resource.ResourceId == day.ResourceId && reservation.Extent.Contains(day.Date));

    void DismissErrorAlert() => showErrorAlert = false;

    Price GetPrice(DraftReservation reservation) =>
        Pricing.GetPrice(options!, DateProvider.Holidays, reservation.Extent, reservation.Resource.Type);
}
