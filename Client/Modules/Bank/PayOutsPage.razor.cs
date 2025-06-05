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
    List<CreditorDto>? creditors;
    bool isInitialized;
    IEnumerable<PayOutDto>? payOuts;
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
            var getPayOutsResponse = await ApiClient.Get<GetPayOutsResponse>("bank/pay-outs");
            payOuts = getPayOutsResponse.IsSuccess ? getPayOutsResponse.Result!.PayOuts : null;
            if (payOuts is not null)
            {
                var alreadyPaidAmounts = payOuts
                    .GroupBy(
                        payOut => payOut.UserIdentity.UserId,
                        (userId, userPayOuts) => (UserId: userId, Amount: userPayOuts.Sum(payOut => payOut.Amount)))
                    .ToDictionary(tuple => tuple.UserId, tuple => tuple.Amount);
                creditors = getCreditorsResponse.Result!.Creditors
                    .Select(creditor => SubtractPayoutFromCreditorAmount(creditor, alreadyPaidAmounts))
                    .Where(creditor => creditor.Payment.Amount > Amount.Zero)
                    .ToList();
            }
        }
        else
        {
            creditors = null;
            payOuts = null;
        }
    }

    static CreditorDto SubtractPayoutFromCreditorAmount(CreditorDto creditor, Dictionary<UserId, Amount> alreadyPaidAmounts) =>
        alreadyPaidAmounts.TryGetValue(creditor.UserInformation.UserId, out var amount)
            ? creditor with { Payment = creditor.Payment with { Amount = creditor.Payment.Amount - amount } }
            : creditor;

    async Task RecordPayOut(CreditorDto creditor)
    {
        DismissAllAlerts();
        var request = new CreatePayOutRequest(creditor.UserInformation.UserId, creditor.Payment.Amount);
        var response = await ApiClient.Post("bank/pay-outs", request);
        if (response.IsSuccess)
        {
            await GetData();
            showSuccessAlert = true;
        }
        else
            showErrorAlert = true;
    }

    async Task DeletePayOut(PayOutDto payOut)
    {
        DismissAllAlerts();
        var response = await ApiClient.Delete($"bank/pay-outs/{payOut.PayOutId}", payOut.ETag.ToString());
        if (response.IsSuccess)
        {
            await GetData();
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
