using Blazorise;
using Frederikskaj2.Reservations.Client.Editors;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Dialogs;

public partial class PayInDialog
{
    int amount;
    LocalDate date;
    DateEditor dateEditor = null!;
    DebtorEditor debtorEditor = null!;
    bool isConfirming;
    Modal modal = null!;
    Debtor? selectedDebtor;
    Validations validations = null!;

    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public IEnumerable<Debtor>? Debtors { get; set; }
    [Parameter] public bool IsDebitHidden { get; set; }
    [Parameter] public EventCallback<(PaymentId PaymentId, LocalDate Date, Amount Amount)> OnConfirm { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    public void Show(Debtor? debtor, LocalDate date)
    {
        selectedDebtor = debtor;
        this.date = date;
        amount = 0;
        isConfirming = false;
        modal.Show();
        Task.Run(() => debtor is not null ? dateEditor.FocusAsync() : debtorEditor.FocusAsync());
    }

    void SetAmount() => amount = selectedDebtor!.Amount.ToInt32();

    void DebtorChanged(Debtor? debtor)
    {
        selectedDebtor = debtor;
        amount = 0;
    }

    void Cancel() => modal.Hide();

    async Task ConfirmAsync()
    {
        await validations.ClearAll();
        if (! await validations.ValidateAll())
            return;

        await validations.ClearAll();
        isConfirming = true;
        await modal.Hide();
    }

    Task OnModalClosed() =>
        isConfirming
            ? OnConfirm.InvokeAsync((selectedDebtor!.PaymentId, date, Amount.FromInt32(amount)))
            : OnCancel.InvokeAsync(null);
}
