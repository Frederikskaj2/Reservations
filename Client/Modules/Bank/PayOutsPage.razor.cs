using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

[Authorize(Roles = nameof(Roles.Bookkeeping))]
partial class PayOutsPage
{
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    IEnumerable<CreditorDto>? creditors;
    IEnumerable<PayOutSummaryDto>? inProgressPayOuts;
    bool isInitialized;
    IEnumerable<PayOutSummaryDto>? otherPayOuts;
    bool showErrorAlert;
    bool showSuccessAlert;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        apartments = await ClientDataProvider.GetApartments();
        await GetData();
        isInitialized = true;
    }

    async ValueTask GetData()
    {
        var getCreditorsResponse = await ApiClient.Get<GetCreditorsResponse>("creditors");
        if (getCreditorsResponse.IsSuccess)
        {
            creditors = getCreditorsResponse.Result!.Creditors;
            var getPayOutsResponse = await ApiClient.Get<GetPayOutsResponse>("bank/pay-outs");
            if (getPayOutsResponse.IsSuccess)
            {
                inProgressPayOuts = getPayOutsResponse.Result!.PayOuts
                    .Where(payOut => payOut.Status is Reservations.Bank.PayOutStatus.InProgress)
                    .OrderByDescending(payOut => payOut.CreatedTimestamp)
                    .ToList();
                otherPayOuts = getPayOutsResponse.Result!.PayOuts
                    .Where(payOut => payOut.Status is not Reservations.Bank.PayOutStatus.InProgress)
                    .OrderByDescending(payOut => payOut.ResolvedTimestamp)
                    .ToList();
            }
            else
            {
                inProgressPayOuts = null;
                otherPayOuts = null;
            }
        }
        else
        {
            creditors = null;
            inProgressPayOuts = null;
            otherPayOuts = null;
        }
    }

    async Task RecordPayOut(CreditorDto creditor)
    {
        DismissAllAlerts();
        var request = new CreatePayOutRequest(creditor.UserInformation.UserId, creditor.Payment.AccountNumber, creditor.Payment.Amount);
        var response = await ApiClient.Post("bank/pay-outs", request);
        if (response.IsSuccess)
        {
            await GetData();
            showSuccessAlert = true;
        }
        else
            showErrorAlert = true;
    }

    void ShowPayOut(PayOutSummaryDto payOut) => NavigationManager.NavigateTo($"til-udbetaling/{payOut.PayOutId}");

    void DismissSuccessAlert() => showSuccessAlert = false;

    void DismissErrorAlert() => showErrorAlert = false;

    void DismissAllAlerts()
    {
        DismissSuccessAlert();
        DismissErrorAlert();
    }
}
