using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

[Authorize(Roles = nameof(Roles.Resident))]
partial class MyOrdersPage
{
    IEnumerable<MyOrderDto>? historyOrders;
    bool isInitialized;
    IEnumerable<MyOrderDto>? orders;
    IReadOnlyDictionary<ResourceId, Resource>? resources;

    [Inject] AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] Formatter Formatter { get; set; } = null!;
    [Inject] NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        resources = await ClientDataProvider.GetResources();
        var response = await ApiClient.Get<GetMyOrdersResponse>("orders/my");
        if (response.IsSuccess)
        {
            orders = response.Result!.Orders.Where(order => !order.IsHistoryOrder).ToList();
            historyOrders = response.Result!.Orders.Where(order => order.IsHistoryOrder).ToList();
        }

        isInitialized = true;
    }

    void EditOrder(MyOrderDto order)
    {
        var url = $"{UrlPath.MyOrders}/{order.OrderId}";
        NavigationManager.NavigateTo(url);
    }

    static string GetReservationStatus(ReservationDto reservation) => reservation.Status switch
    {
        ReservationStatus.Reserved => "Afventer betaling",
        ReservationStatus.Abandoned => "Aflyst",
        ReservationStatus.Confirmed => "Betalt",
        ReservationStatus.Cancelled => "Aflyst",
        ReservationStatus.Settled => "Afsluttet",
        _ => "Ukendt",
    };
}
