using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

[Authorize(Roles = nameof(Roles.Resident))]
public partial class MyTransactionsPage
{
    Amount balance;
    bool isInitialized;
    GetMyTransactionsResponse? myTransactions;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var response = await ApiClient.Get<GetMyTransactionsResponse>("transactions");
        if (response.IsSuccess)
        {
            myTransactions = response.Result;
            balance = myTransactions!.Transactions.Sum(transaction => transaction.Amount);
        }
        isInitialized = true;
    }
}
