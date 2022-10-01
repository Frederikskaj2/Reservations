using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Policy = Policies.ViewOrders)]
public partial class OwnerOrdersPage
{
    bool isInitialized;
    List<Order>? orders;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await GetOrders();
        isInitialized = true;

        async Task GetOrders()
        {
            var response = await ApiClient.GetAsync<IEnumerable<Order>>("orders/owner");
            orders = response.IsSuccess ? response.Result!.ToList() : new List<Order>();
        }
    }
}
