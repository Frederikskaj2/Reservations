using Blazorise;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

partial class UpdatePayOutAccountNumberDialog
{
    bool isConfirming;
    Modal modal = null!;
    string? newAccountNumber;
    string? originalAccountNumber;

    [Parameter] public string? AccountNumber { get; set; }
    [Parameter] public EventCallback<string> OnConfirm { get; set; }

    protected override void OnParametersSet()
    {
        originalAccountNumber = AccountNumber;
        newAccountNumber = "";
    }

    public Task Show()
    {
        isConfirming = false;
        return modal.Show();
    }

    Task Cancel() => modal.Hide();

    Task Confirm()
    {
        isConfirming = true;
        return modal.Hide();
    }

    Task OnModalClosed()
    {
        if (!isConfirming)
            return Task.CompletedTask;
        return OnConfirm.InvokeAsync(newAccountNumber!.Trim());
    }
}
