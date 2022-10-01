using Frederikskaj2.Reservations.Client.Dialogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Apartment = Frederikskaj2.Reservations.Shared.Core.Apartment;
using ApartmentId = Frederikskaj2.Reservations.Shared.Core.ApartmentId;
using Debtor = Frederikskaj2.Reservations.Shared.Core.Debtor;
using IDateProvider = Frederikskaj2.Reservations.Shared.Core.IDateProvider;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.Bookkeeping))]
public partial class DebtorsPage
{
    Amount amount;
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    List<Debtor>? debtors;
    bool isInitialized;
    List<Debtor>? otherDebtors;
    PayInDialog otherPayInDialog = null!;
    PayInDialog payInDialog = null!;
    PaymentId paymentId;
    bool showErrorAlert;
    bool showSuccessAlert;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var apartments = await DataProvider.GetApartmentsAsync();
        if (apartments is not null)
        {
            this.apartments = apartments.ToDictionary(apartment => apartment.ApartmentId);
            var response = await ApiClient.GetAsync<IEnumerable<Debtor>>("debtors");
            if (response.IsSuccess)
            {
                var allDebtors = response.Result!;
                debtors = allDebtors.Where(debtor => debtor.Amount > Amount.Zero).ToList();
                otherDebtors = allDebtors.Where(debtor => debtor.Amount == Amount.Zero).ToList();
            }
        }
        isInitialized = true;
    }

    void RecordPayIn(Debtor debtor)
    {
        DismissSuccessAlert();
        DismissErrorAlert();
        payInDialog.Show(debtor, DateProvider.Today);
    }

    void RecordPayIn()
    {
        DismissSuccessAlert();
        DismissErrorAlert();
        otherPayInDialog.Show(null, DateProvider.Today);
    }

    async Task OnPayInConfirmAsync((Reservations.Shared.Core.PaymentId PaymentId, LocalDate Date, Reservations.Shared.Core.Amount Amount) tuple)
    {
        var request = new PayInRequest { Date = tuple.Date, Amount = tuple.Amount };
        var requestUri = $"payments/{tuple.PaymentId}";
        var response = await ApiClient.PostAsync<Debtor>(requestUri, request);
        if (response.IsSuccess)
        {
            var debtor = response.Result!;
            var index = debtors!.FindIndex(d => d.PaymentId == debtor.PaymentId);
            if (index >= 0)
            {
                if (debtor.Amount <= Amount.Zero)
                {
                    debtors.RemoveAt(index);
                    otherDebtors = otherDebtors!.Append(debtor).OrderBy(d => d.PaymentId).ToList();
                }
                else
                    debtors[index] = debtors[index] with { Amount = debtor.Amount };
            }

            paymentId = tuple.PaymentId;
            amount = tuple.Amount;
            showSuccessAlert = true;
        }
        else
            showErrorAlert = true;
    }

    void DismissSuccessAlert() => showSuccessAlert = false;
    void DismissErrorAlert() => showErrorAlert = false;
}
