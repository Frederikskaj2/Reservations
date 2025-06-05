using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

[Authorize(Policy = Policy.ViewOrders)]
partial class OrdersPage
{
    List<OrderSummaryDto>? confirmedOrders;
    bool isInitialized;
    List<OrderSummaryDto>? reservedOrders;
    List<OrderSummaryDto>? unsettledOrders;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var orders = await GetOrders();
        var lookup = orders.ToLookup(order => order.Category);
        reservedOrders = lookup.Contains(OrderCategory.Reserved) ? lookup[OrderCategory.Reserved].ToList() : [];
        confirmedOrders = lookup.Contains(OrderCategory.Confirmed) ? lookup[OrderCategory.Confirmed].ToList() : [];
        unsettledOrders = lookup.Contains(OrderCategory.NeedsSettlement) ? lookup[OrderCategory.NeedsSettlement].ToList() : [];

        isInitialized = true;

        async ValueTask<IEnumerable<OrderSummaryDto>> GetOrders()
        {
            var response = await ApiClient.Get<GetOrdersResponse>("orders?type=Resident");
            return response.IsSuccess ? response.Result!.Orders : [];
        }
    }
}
