using Blazorise;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

partial class PayOutNoteDialog
{
    Modal modal = null!;
    string text = "";

    [Parameter] public EventCallback<string> OnConfirm { get; set; }

    public Task Show()
    {
        text = "";
        return modal.Show();
    }

    async Task Confirm()
    {
        await modal.Hide();
        await OnConfirm.InvokeAsync(text);
    }

    Task Cancel() => modal.Hide();
}
