using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

[Authorize(Policy = Policy.ViewOrders)]
partial class OwnerOrdersPage
{
    bool isInitialized;
    List<OrderSummaryDto>? orders;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await GetOrders();
        isInitialized = true;

        async Task GetOrders()
        {
            var response = await ApiClient.Get<GetOrdersResponse>("orders?type=Owner");
            orders = response.IsSuccess ? response.Result!.Orders.ToList() : [];
        }
    }
}
