using Frederikskaj2.Reservations.Client.Dialogs;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Policy = Policies.ViewOrders)]
public partial class OrderPage
{
    Dictionary<ApartmentId, Apartment>? apartments;
    bool canHandleOrders;
    bool canWaiveFee;
    bool isInitialized;
    OrderingOptions? options;
    OrderDetails? order;
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
        options = await DataProvider.GetOptionsAsync();
        apartments = (await DataProvider.GetApartmentsAsync())?.ToDictionary(apartment => apartment.ApartmentId);
    }

    protected override async Task OnParametersSetAsync()
    {
        isInitialized = false;

        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        canHandleOrders = authenticationState.User.IsInRole(nameof(Roles.OrderHandling));

        var url = $"orders/any/{OrderId}";
        var response = await ApiClient.GetAsync<OrderDetails>(url);
        order = response.Result;

        if (order is not null && options is not null)
            canWaiveFee = canHandleOrders && order.Reservations.Any(reservation =>
                ReservationPolicies.CanReservationBeCancelled(options, DateProvider.Today, reservation.Status, reservation.Extent, true));

        isInitialized = true;
    }

    void SettleReservation(ReservationIndex reservationIndex)
    {
        DismissAllAlerts();
        settlementDialog.Show(order!.UserInformation.UserId, order.OrderId, order.Reservations.ElementAt(reservationIndex.ToInt32()), reservationIndex);
    }

    async Task SubmitAsync((string? AccountNumber, HashSet<ReservationIndex> CancelledReservations, bool WaiveFee, bool AllowCancellationWithoutFee) tuple)
    {
        DismissAllAlerts();

        var (accountNumber, cancelledReservations, waiveFee, allowCancellationWithoutFee) = tuple;
        var request = new UpdateUserOrderRequest
        {
            UserId = order!.UserInformation.UserId,
            AccountNumber = accountNumber,
            CancelledReservations = cancelledReservations,
            WaiveFee = waiveFee,
            AllowCancellationWithoutFee = allowCancellationWithoutFee
        };
        var url = $"orders/user/{order.OrderId}";
        var response = await ApiClient.PatchAsync<OrderDetails>(url, request);
        if (response.IsSuccess)
        {
            order = response.Result;
            showSuccessAlert = true;
        }
        else
            showErrorAlert = true;
    }

    async Task OnSettleConfirmAsync((OrderId OrderId, SettleReservationRequest Request) tuple)
    {
        var (orderId, request) = tuple;
        var requestUri = $"orders/user/{orderId}/settle-reservation";
        var response = await ApiClient.PostAsync<OrderDetails>(requestUri, request);
        if (response.IsSuccess)
        {
            order = response.Result;
            showSuccessAlert = true;
        }
        else
            showErrorAlert = true;
    }

    async Task SubmitOwnerOrderAsync((string? Description, HashSet<ReservationIndex> CancelledReservations, bool IsCleaningRequired) tuple)
    {
        DismissAllAlerts();

        var (description, cancelledReservations, isCleaningRequired) = tuple;
        var request = new UpdateOwnerOrderRequest
        {
            Description = description,
            CancelledReservations = cancelledReservations,
            IsCleaningRequired = isCleaningRequired
        };
        var url = $"orders/owner/{order!.OrderId}";
        var response = await ApiClient.PatchAsync<OrderDetails>(url, request);
        if (response.IsSuccess)
        {
            order = response.Result;
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
}
