using Blazorise;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class ConfirmDeleteUserDialog
{
    EmailAddress email;
    string? fullName;
    Modal modal = null!;
    Button submitButton = null!;

    [Parameter] public EventCallback OnConfirm { get; set; }

    public async ValueTask Show(UserDto user)
    {
        email = user.Identity.Email;
        fullName = user.Identity.FullName;
        await modal.Show();
        await submitButton.ElementRef.FocusAsync();
    }

    async Task Confirm()
    {
        await modal.Hide();
        await OnConfirm.InvokeAsync(arg: null);
    }

    Task Cancel() => modal.Hide();
}
