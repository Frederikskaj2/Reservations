using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Orders.ReservationPolicies;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

[Authorize(Policy = Policy.ViewOrders)]
partial class OrderPage
{
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    bool canHandleOrders;
    bool canWaiveFee;
    bool isInitialized;
    OrderingOptions? options;
    OrderDetailsDto? order;
    SettlementDialog settlementDialog = null!;
    bool showErrorAlert;
    bool showSuccessAlert;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;

    [Parameter] public int OrderId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        options = await DataProvider.GetOptions();
        apartments = await DataProvider.GetApartments();
    }

    protected override async Task OnParametersSetAsync()
    {
        isInitialized = false;

        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        canHandleOrders = authenticationState.User.IsInRole(nameof(Roles.OrderHandling));

        var url = $"orders/{OrderId}";
        var response = await ApiClient.Get<GetOrderResponse>(url);
        order = response.Result!.Order;

        if (order is not null && options is not null)
            canWaiveFee = canHandleOrders && order.Reservations.Any(
                reservation => CanReservationBeCancelled(options, DateProvider.Today, reservation.Status, reservation.Extent, alwaysAllowCancellation: true));

        isInitialized = true;
    }

    Task SettleReservation(ReservationIndex reservationIndex)
    {
        DismissAllAlerts();
        return settlementDialog.Show(order!.OrderId, order.Reservations.ElementAt(reservationIndex.ToInt32()), reservationIndex);
    }

    async Task Submit((string? AccountNumber, HashSet<ReservationIndex> CancelledReservations, bool WaiveFee, bool AllowCancellationWithoutFee) tuple)
    {
        DismissAllAlerts();

        var (accountNumber, cancelledReservations, waiveFee, allowCancellationWithoutFee) = tuple;
        var request = new UpdateResidentOrderRequest(AccountNumber: accountNumber, CancelledReservations: cancelledReservations, WaiveFee: waiveFee,
            AllowCancellationWithoutFee: allowCancellationWithoutFee);
        var url = $"orders/resident/{order!.OrderId}";
        var response = await ApiClient.Patch<UpdateResidentOrderResponse>(url, request);
        if (response.IsSuccess)
        {
            order = response.Result!.Order;
            showSuccessAlert = true;
        }
        else
            showErrorAlert = true;
    }

    async Task OnSettleConfirm((OrderId OrderId, SettleReservationRequest Request) tuple)
    {
        var (orderId, request) = tuple;
        var requestUri = $"orders/resident/{orderId}/settle-reservation";
        var response = await ApiClient.Post<SettleReservationResponse>(requestUri, request);
        if (response.IsSuccess)
        {
            order = response.Result!.Order;
            showSuccessAlert = true;
        }
        else
            showErrorAlert = true;
    }

    async Task SubmitOwnerOrder((string? Description, HashSet<ReservationIndex> CancelledReservations, bool IsCleaningRequired) tuple)
    {
        DismissAllAlerts();

        var (description, cancelledReservations, isCleaningRequired) = tuple;
        var request = new UpdateOwnerOrderRequest(Description: description, CancelledReservations: cancelledReservations,
            IsCleaningRequired: isCleaningRequired);
        var url = $"orders/owner/{order!.OrderId}";
        var response = await ApiClient.Patch<UpdateOwnerOrderResponse>(url, request);
        if (response.IsSuccess)
        {
            order = response.Result!.Order;
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
}
