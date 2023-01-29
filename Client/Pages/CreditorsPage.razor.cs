using Frederikskaj2.Reservations.Client.Dialogs;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Globalization;
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
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    bool isInitialized;
    List<Creditor>? creditors;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

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

    void RecordPayOut(Creditor creditor) =>
        NavigationManager.NavigateTo(string.Format(CultureInfo.InvariantCulture, Urls.PayOut, creditor.UserInformation.UserId));
}
