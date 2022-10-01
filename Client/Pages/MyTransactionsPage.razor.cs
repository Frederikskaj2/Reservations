using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.Resident))]
public partial class MyTransactionsPage
{
    Amount balance;
    bool isInitialized;
    MyTransactions? transactions;
    UserId userId;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public TokenAuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        userId = state.User.UserId();
        var response = await ApiClient.GetAsync<MyTransactions>("transactions");
        if (response.IsSuccess)
        {
            transactions = response.Result;
            balance = transactions!.Transactions.Sum(transaction => transaction.Amount);
        }
        isInitialized = true;
    }
}
