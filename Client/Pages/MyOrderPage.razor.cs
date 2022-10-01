using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.Resident))]
public partial class MyOrderPage
{
    string? accountNumber;
    Dictionary<ApartmentId, Apartment>? apartments;
    bool isInitialized;
    bool linkBanquetFacilitiesRules;
    bool linkBedroomRules;
    MyOrder? order;
    string? originalAccountNumber;
    List<ReservationLockBoxCode>? lockBoxCodes;
    IReadOnlyDictionary<ResourceId, Resource>? resources;
    bool showErrorAlert;
    bool showSuccessAlert;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public SignOutService SignOutService { get; set; } = null!;

    [Parameter] public int OrderId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        apartments = (await DataProvider.GetApartmentsAsync())?.ToDictionary(apartment => apartment.ApartmentId);
        await GetAccountNumberAsync();

        var url = $"orders/my/{OrderId}";
        var response = await ApiClient.GetAsync<MyOrder>(url);
        if (response.IsSuccess)
        {
            order = response.Result!;

            resources = await DataProvider.GetResourcesAsync();
            if (resources is not null)
            {
                linkBanquetFacilitiesRules = order!.Reservations!.Any(
                    reservation => resources![reservation.ResourceId].Type is ResourceType.BanquetFacilities &&
                                   reservation.Status is ReservationStatus.Reserved or ReservationStatus.Confirmed);
                linkBedroomRules = order.Reservations!.Any(
                    reservation => resources![reservation.ResourceId].Type is ResourceType.Bedroom &&
                                   reservation.Status is ReservationStatus.Reserved or ReservationStatus.Confirmed);
                lockBoxCodes = order.Reservations!
                    .SelectMany(
                        reservation => reservation.LockBoxCodes ?? Enumerable.Empty<DatedLockBoxCode>(),
                        (reservation, lockBoxCode) => new ReservationLockBoxCode(resources[reservation.ResourceId].Name, lockBoxCode.Date, lockBoxCode.Code))
                    .ToList();
            }
        }

        isInitialized = true;
    }

    async Task GetAccountNumberAsync()
    {
        var response = await ApiClient.GetAsync<MyUser>("user");
        if (response.IsSuccess)
        {
            originalAccountNumber = response.Result!.AccountNumber;
            accountNumber = originalAccountNumber;
        }
    }

    async Task SubmitAsync((string AccountNumber, HashSet<ReservationIndex> CancelledReservations, bool WaiveFee, bool AllowCancellationWithoutFee) tuple)
    {
        DismissAllAlerts();

        var request = new UpdateMyOrderRequest
        {
            AccountNumber = tuple.AccountNumber,
            CancelledReservations = tuple.CancelledReservations,
            WaiveFee = tuple.WaiveFee
        };
        var url = $"orders/my/{order!.OrderId}";
        var response = await ApiClient.PatchAsync<UpdateMyOrderResult>(url, request);
        if (response.IsSuccess)
        {
            if (response.Result!.IsUserDeleted)
            {
                await SignOutService.SignOutAsync();
                return;
            }

            order = response.Result!.Order;
            accountNumber = request.AccountNumber;
            showSuccessAlert = true;
        }
        else
            showErrorAlert = true;
    }

    void DismissSuccessAlert() => showSuccessAlert = false;

    void DismissErrorAlert() => showErrorAlert = false;

    void DismissAllAlerts()
    {
        DismissSuccessAlert();
        DismissErrorAlert();
    }

    Apartment GetApartment(ApartmentId apartmentId) => apartments![apartmentId];

    record ReservationLockBoxCode(string ResourceName, LocalDate Date, string Code);
}
