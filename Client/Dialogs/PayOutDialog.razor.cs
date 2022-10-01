using Blazorise;
using Frederikskaj2.Reservations.Client.Editors;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Dialogs;

public partial class PayOutDialog
{
    int amount;
    AmountEditor amountEditor = null!;
    LocalDate date;
    string? fullName;
    bool isConfirming;
    int maximumAmount;
    Modal modal = null!;
    UserId userId;
    Validations validations = null!;

    [Parameter] public EventCallback<(UserId UserId, LocalDate Date, Amount Amount)> OnConfirm { get; set; }

    [Parameter] public EventCallback OnCancel { get; set; }

    public void Show(UserId userId, string fullName, LocalDate date, Amount amount)
    {
        this.userId = userId;
        this.date = date;
        this.fullName = fullName;
        maximumAmount = amount.ToInt32();
        this.amount = maximumAmount;
        isConfirming = false;
        modal.Show();
    }

    void Cancel() => modal.Hide();

    async Task ConfirmAsync()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        isConfirming = true;
        await modal.Hide();
    }

    Task OnModalClosed() => isConfirming ? OnConfirm.InvokeAsync((userId, date, Amount.FromInt32(amount))) : OnCancel.InvokeAsync(null);
}
