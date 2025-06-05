using Blazorise;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class ConfirmDeleteMyUserDialog
{
    Modal modal = null!;

    [Parameter] public EventCallback OnConfirm { get; set; }

    public Task Show() => modal.Show();

    async Task Confirm()
    {
        await modal.Hide();
        await OnConfirm.InvokeAsync(arg: null);
    }

    Task Cancel() => modal.Hide();
}
