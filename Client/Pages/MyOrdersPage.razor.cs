using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.Resident))]
public partial class MyOrdersPage
{
    IEnumerable<MyOrder>? historyOrders;
    bool isInitialized;
    IEnumerable<MyOrder>? orders;
    IReadOnlyDictionary<ResourceId, Resource>? resources;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        resources = await ClientDataProvider.GetResourcesAsync();
        var response = await ApiClient.GetAsync<IEnumerable<MyOrder>>("orders/my");
        if (response.IsSuccess)
        {
            orders = response.Result!.Where(order => !order.IsHistoryOrder).ToList();
            historyOrders = response.Result!.Where(order => order.IsHistoryOrder).ToList();
        }

        isInitialized = true;
    }

    void EditOrder(MyOrder order)
    {
        var url = $"{Urls.MyOrders}/{order.OrderId}";
        NavigationManager.NavigateTo(url);
    }

    static string GetReservationStatus(Reservation reservation) => reservation.Status switch
    {
        ReservationStatus.Reserved => "Afventer betaling",
        ReservationStatus.Abandoned => "Aflyst",
        ReservationStatus.Confirmed => "Betalt",
        ReservationStatus.Cancelled => "Aflyst",
        ReservationStatus.Settled => "Afsluttet",
        _ => "Ukendt"
    };
}
