using Blazorise;
using Frederikskaj2.Reservations.Client.Shared;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.Resident))]
public partial class Checkout2Page
{
    IEnumerable<Apartment>? apartments;
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
    [Inject] public DraftOrder DraftOrder { get; init; } = null!;
    [Inject] public Formatter Formatter { get; init; } = null!;
    [Inject] public NavigationManager NavigationManager { get; init; } = null!;
    [Inject] public UserOrderInformation OrderInformation { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        options = await DataProvider.GetOptionsAsync();
        apartments = await DataProvider.GetApartmentsAsync();
        linkBanquetFacilitiesRules =
            DraftOrder.Reservations.Any(reservation => reservation.Resource.Type == ResourceType.BanquetFacilities);
        linkBedroomRules = DraftOrder.Reservations.Any(reservation => reservation.Resource.Type == ResourceType.Bedroom);
        isInitialized = true;
    }

    protected override void OnParametersSet() =>
        (totalRentAndCleaning, totalDeposit) = DraftOrder.Reservations
            .Select(GetPrice)
            .Aggregate(
                (RentAndCleaning: Amount.Zero, Deposit: Amount.Zero),
                (tuple, price) => (tuple.RentAndCleaning + price.Rent + price.Cleaning, tuple.Deposit + price.Deposit));

    async Task SubmitAsync()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        DismissErrorAlert();

        var request = new PlaceMyOrderRequest
        {
            FullName = OrderInformation.FullName,
            Phone = OrderInformation.Phone,
            ApartmentId = OrderInformation.ApartmentId,
            Reservations = DraftOrder.GetReservationRequests().ToArray(),
            AccountNumber = OrderInformation.AccountNumber
        };
        progressBar.Start();
        var response = await ApiClient.PostAsync<MyOrder>("orders/my", request);
        progressBar.Stop();
        if (response.IsSuccess)
        {
            DataProvider.CurrentOrder = response.Result;
            DraftOrder.Clear();
            NavigationManager.NavigateTo(Urls.Checkout3);
        }
        else if (response.Problem!.Status == HttpStatusCode.Conflict)
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
            var response2 = await ApiClient.GetAsync<IEnumerable<MyReservedDay>>("reserved-days/my");
            // TODO: Check result! Refactor!!
            var reservedDays = response2.Result!.Where(reservedDay => fromDate <= reservedDay.Date && reservedDay.Date <= toDate).ToList();
            var conflictedReservations =
                from reservation in DraftOrder.Reservations
                let isConflicted = reservedDays.Any(day => reservation.Resource.ResourceId == day.ResourceId && reservation.Extent.Contains(day.Date))
                where isConflicted
                select reservation;
            foreach (var reservation in conflictedReservations.ToList())
                DraftOrder.RemoveReservation(reservation);
            NavigationManager.NavigateTo(Urls.CheckoutConflict);
        }

        showErrorAlert = true;
    }

    void DismissErrorAlert() => showErrorAlert = false;

    Price GetPrice(DraftReservation reservation) =>
        Pricing.GetPrice(options!, DataProvider.Holidays, reservation.Extent, reservation.Resource.Type);
}
