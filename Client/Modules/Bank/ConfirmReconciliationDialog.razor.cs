using Blazorise;
using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

partial class ConfirmReconciliationDialog
{
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    Modal modal = null!;

    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public EventCallback<(BankTransactionDto, PayOutDto)> OnConfirmPayOut { get; set; }
    [Parameter] public EventCallback<(BankTransactionDto, ResidentDto)> OnConfirmReconciliation { get; set; }

    PayOutDto? PayOut { get; set; }
    ResidentDto? Resident { get; set; }
    BankTransactionDto? Transaction { get; set; }

    protected override async Task OnInitializedAsync() => apartments = await ClientDataProvider.GetApartments();

    public Task Show(BankTransactionDto transaction, ResidentDto resident)
    {
        Transaction = transaction;
        Resident = resident;
        PayOut = null;
        return modal.Show();
    }

    public Task Show(BankTransactionDto transaction, ResidentDto resident, PayOutDto payOut)
    {
        Transaction = transaction;
        Resident = resident;
        PayOut = payOut;
        return modal.Show();
    }

    Task Cancel() => modal.Hide();

    async Task Confirm()
    {
        await modal.Hide();
        await (PayOut is null ? OnConfirmReconciliation.InvokeAsync((Transaction!, Resident!)) : OnConfirmPayOut.InvokeAsync((Transaction!, PayOut!)));
    }
}
