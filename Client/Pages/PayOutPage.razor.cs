using Blazorise;
using Frederikskaj2.Reservations.Client.Editors;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System.Net;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.Bookkeeping))]
public partial class PayOutPage
{
    int amount;
    AmountEditor amountEditor = null!;
    Creditor? creditor;
    LocalDate date;
    bool isComplete;
    bool isInitialized;
    int maximumAmount;
    bool notFound;
    bool showErrorAlert;
    bool showSuccessAlert;
    UserTransactions? transactions;
    Validations validations = null!;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;

    [Parameter] public int UserId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        date = DateProvider.Today;
        var creditorResponse = await ApiClient.GetAsync<Creditor>($"creditors/{UserId}");
        if (creditorResponse.IsSuccess)
        {
            creditor = creditorResponse.Result!;
            amount = creditor.Amount.ToInt32();
            maximumAmount = amount;
            var transactionsResponse = await ApiClient.GetAsync<UserTransactions>($"users/{UserId}/transactions");
            if (transactionsResponse.IsSuccess)
                transactions = transactionsResponse.Result;
        }
        else if (creditorResponse.Problem!.Status is HttpStatusCode.NotFound)
            notFound = true;
        isInitialized = true;
    }

    async Task PayOutAsync()
    {
        isComplete = true;

        DismissAllAlerts();

        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        var request = new PayOutRequest { Date = date, Amount = Amount.FromInt32(amountEditor.Value) };
        var requestUri = $"users/{creditor!.UserInformation.UserId}/pay-out";
        var response = await ApiClient.PostAsync<Creditor>(requestUri, request);
        if (response.IsSuccess)
            showSuccessAlert = true;
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
