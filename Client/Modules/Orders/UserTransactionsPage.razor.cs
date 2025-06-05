using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

[Authorize(Policy = Policy.ViewUsers)]
partial class UserTransactionsPage
{
    bool isInitialized;
    TransactionId? selectedTransactionId;
    GetUserTransactionsResponse? transactions;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    [Parameter] public int UserId { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        var response = await ApiClient.Get<GetUserTransactionsResponse>($"users/{UserId}/transactions");
        if (response.IsSuccess)
            transactions = response.Result;
        isInitialized = true;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;
        var url = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        if (url.Fragment.Length is 0)
            return;
        // This code expects the fragment to be #t-<transaction ID>.
        selectedTransactionId = TransactionId.FromInt32(int.Parse(url.Fragment[3..], CultureInfo.InvariantCulture));
    }
}
