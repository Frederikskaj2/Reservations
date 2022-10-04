using Frederikskaj2.Reservations.Client.Dialogs;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Apartment = Frederikskaj2.Reservations.Shared.Core.Apartment;
using ApartmentId = Frederikskaj2.Reservations.Shared.Core.ApartmentId;
using Creditor = Frederikskaj2.Reservations.Shared.Core.Creditor;
using IDateProvider = Frederikskaj2.Reservations.Shared.Core.IDateProvider;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.Bookkeeping))]
public partial class CreditorsPage
{
    string? accountNumber;
    Amount amount;
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    bool isInitialized;
    PayOutDialog payOutDialog = null!;
    List<Creditor>? creditors;
    bool showErrorAlert;
    bool showSuccessAlert;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var apartments = await ClientDataProvider.GetApartmentsAsync();
        if (apartments is not null)
        {
            this.apartments = apartments.ToDictionary(apartment => apartment.ApartmentId);
            var response = await ApiClient.GetAsync<IEnumerable<Creditor>>("creditors");
            if (response.IsSuccess)
                creditors = response.Result!.OrderBy(creditor => creditor.UserInformation.ApartmentId?.ToInt32() ?? 999).ToList();
        }
        isInitialized = true;
    }

    void RecordPayOut(Creditor creditor)
    {
        DismissSuccessAlert();
        DismissErrorAlert();
        accountNumber = creditor.AccountNumber;
        payOutDialog.Show(creditor.UserInformation.UserId, creditor.UserInformation.FullName, DateProvider.Today, creditor.Amount);
    }

    async Task OnPaymentConfirmAsync((UserId UserId, LocalDate Date, Amount Amount) tuple)
    {
        var request = new PayOutRequest { Date = tuple.Date, Amount = tuple.Amount };
        var requestUri = $"users/{tuple.UserId}/pay-out";
        var response = await ApiClient.PostAsync<Creditor>(requestUri, request);
        if (response.IsSuccess)
        {
            var result = response.Result!;
            var index = creditors!.FindIndex(creditor => creditor.UserInformation.UserId == result.UserInformation.UserId);
            if (index >= 0)
            {
                if (result.Amount == Amount.Zero)
                    creditors.RemoveAt(index);
                else
                    creditors[index] = creditors[index] with { Amount = result.Amount };
            }
            amount = tuple.Amount;
            showSuccessAlert = true;
        }
        else
            showErrorAlert = true;
    }

    void DismissSuccessAlert() => showSuccessAlert = false;
    void DismissErrorAlert() => showErrorAlert = false;
}
