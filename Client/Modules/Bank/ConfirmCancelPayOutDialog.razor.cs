using Blazorise;
using Frederikskaj2.Reservations.Bank;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

partial class ConfirmCancelPayOutDialog
{
    Modal modal = null!;
    Button submitButton = null!;

    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public EventCallback OnConfirm { get; set; }
    [Parameter] public PayOutDetailsDto? PayOut { get; set; }

    public async Task Show()
    {
        if (PayOut is not null)
        {
            await modal.Show();
            await submitButton.ElementRef.FocusAsync();
        }
    }

    async Task Confirm()
    {
        await modal.Hide();
        await OnConfirm.InvokeAsync(arg: null);
    }

    Task Cancel() => modal.Hide();
}
