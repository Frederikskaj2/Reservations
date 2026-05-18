using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

[Authorize(Roles = nameof(Roles.Resident))]
partial class MyOrderPage
{
    string? accountNumber;
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    List<ReservationEntryCode>? entryCodes;
    bool isInitialized;
    bool linkBanquetFacilitiesRules;
    bool linkBedroomRules;
    MyOrderDto? order;
    string? originalAccountNumber;
    bool showErrorAlert;
    bool showSuccessAlert;

    [Inject] AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] Formatter Formatter { get; set; } = null!;
    [Inject] SignOutService SignOutService { get; set; } = null!;

    [Parameter] public int OrderId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        apartments = await DataProvider.GetApartments();
        await GetAccountNumber();

        var url = $"orders/my/{OrderId}";
        var response = await ApiClient.Get<GetMyOrderResponse>(url);
        if (response.IsSuccess)
        {
            order = response.Result!.Order;

            var resources = await DataProvider.GetResources();
            linkBanquetFacilitiesRules = order!.Reservations.Any(
                reservation => resources![reservation.ResourceId].Type is ResourceType.BanquetFacilities &&
                               reservation.Status is ReservationStatus.Reserved or ReservationStatus.Confirmed);
            linkBedroomRules = order.Reservations.Any(
                reservation => resources![reservation.ResourceId].Type is ResourceType.Bedroom &&
                               reservation.Status is ReservationStatus.Reserved or ReservationStatus.Confirmed);
            entryCodes = order.Reservations
                .Where(reservation => reservation.EntryCode is not null)
                .Select(reservation => new ReservationEntryCode(resources[reservation.ResourceId].Name, reservation.EntryCode.ToString()!))
                .ToList();
        }

        isInitialized = true;
    }

    async Task GetAccountNumber()
    {
        var response = await ApiClient.Get<GetMyUserResponse>("user");
        if (response.IsSuccess)
        {
            originalAccountNumber = response.Result!.AccountNumber;
            accountNumber = originalAccountNumber;
        }
    }

    async Task Submit((string AccountNumber, HashSet<ReservationIndex> CancelledReservations, bool WaiveFee, bool AllowCancellationWithoutFee) tuple)
    {
        DismissAllAlerts();

        var request = new UpdateMyOrderRequest(AccountNumber: tuple.AccountNumber, CancelledReservations: tuple.CancelledReservations);
        var url = $"orders/my/{order!.OrderId}";
        var response = await ApiClient.Patch<UpdateMyOrderResponse>(url, request);
        if (response.IsSuccess)
        {
            if (response.Result!.IsUserDeleted)
            {
                await SignOutService.SignOut();
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

    record ReservationEntryCode(string ResourceName, string Code);
}
