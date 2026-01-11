using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

partial class PayOutDetails
{
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    ConfirmCancelPayOutDialog confirmCancelPayOutDialog = null!;
    LocalDate today;
    UpdatePayOutAccountNumberDialog updatePayOutAccountNumberDialog = null!;

    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public ITimeConverter TimeConverter { get; set; } = null!;

    [Parameter] public EventCallback OnCancelPayOut { get; set; }
    [Parameter] public EventCallback<string> OnUpdateAccountNumber { get; set; }
    [Parameter] public PayOutDetailsDto? PayOut { get; set; }

    protected override async Task OnInitializedAsync() => apartments = await ClientDataProvider.GetApartments();

    protected override void OnParametersSet() => today = DateProvider.Today;

    Task UpdateAccountNumber() => updatePayOutAccountNumberDialog.Show();

    Task CancelPayOut() => confirmCancelPayOutDialog.Show();
}

