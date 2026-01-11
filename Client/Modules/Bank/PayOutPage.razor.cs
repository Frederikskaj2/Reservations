using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

[Authorize(Roles = nameof(Roles.Bookkeeping))]
partial class PayOutPage
{
    bool isInitialized;
    PayOutDetailsDto? payOut;
    bool showErrorAlert;
    bool showSuccessAlert;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;

    [Parameter] public int PayOutId { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        isInitialized = false;

        var url = $"bank/pay-outs/{PayOutId}";
        var response = await ApiClient.Get<GetPayOutResponse>(url);
        payOut = response.Result!.PayOut;

        isInitialized = true;
    }

    async Task OnUpdateAccountNumber(string accountNumber)
    {
        DismissAllAlerts();
        var request = new UpdatePayOutAccountNumberRequest(accountNumber);
        var response = await ApiClient.Patch<UpdatePayOutAccountNumberResponse>($"bank/pay-outs/{payOut!.PayOutId}", request);
        if (response.IsSuccess)
        {
            payOut = response.Result!.PayOut;
            showSuccessAlert = true;
        }
        else
            showErrorAlert = true;
    }

    async Task OnCancelPayOut()
    {
        DismissAllAlerts();
        var response = await ApiClient.Post<CancelPayOutResponse>($"bank/pay-outs/{payOut!.PayOutId}/cancel");
        if (response.IsSuccess)
        {
            payOut = response.Result!.PayOut;
            showSuccessAlert = true;
        }
        else
            showErrorAlert = true;
    }

    async Task OnAddNote(string note)
    {
        DismissAllAlerts();
        var request = new AddPayOutNoteRequest(note);
        var response = await ApiClient.Post<AddPayOutNoteResponse>($"bank/pay-outs/{payOut!.PayOutId}/notes", request);
        if (response.IsSuccess)
        {
            payOut = response.Result!.PayOut;
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
}
