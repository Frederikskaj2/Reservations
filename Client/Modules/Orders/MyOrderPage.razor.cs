using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

[Authorize(Roles = nameof(Roles.Resident))]
partial class MyOrderPage
{
    string? accountNumber;
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    bool isInitialized;
    bool linkBanquetFacilitiesRules;
    bool linkBedroomRules;
    MyOrderDto? order;
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
        apartments = await DataProvider.GetApartments();
        await GetAccountNumber();

        var url = $"orders/my/{OrderId}";
        var response = await ApiClient.Get<GetMyOrderResponse>(url);
        if (response.IsSuccess)
        {
            order = response.Result!.Order;

            resources = await DataProvider.GetResources();
            if (resources is not null)
            {
                linkBanquetFacilitiesRules = order!.Reservations.Any(
                    reservation => resources![reservation.ResourceId].Type is ResourceType.BanquetFacilities &&
                                   reservation.Status is ReservationStatus.Reserved or ReservationStatus.Confirmed);
                linkBedroomRules = order.Reservations.Any(
                    reservation => resources![reservation.ResourceId].Type is ResourceType.Bedroom &&
                                   reservation.Status is ReservationStatus.Reserved or ReservationStatus.Confirmed);
                lockBoxCodes = order.Reservations
                    .SelectMany(
                        reservation => reservation.LockBoxCodes,
                        (reservation, lockBoxCode) => new ReservationLockBoxCode(resources[reservation.ResourceId].Name, lockBoxCode.Date, lockBoxCode.Code))
                    .ToList();
            }
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

    record ReservationLockBoxCode(string ResourceName, LocalDate Date, string Code);
}
