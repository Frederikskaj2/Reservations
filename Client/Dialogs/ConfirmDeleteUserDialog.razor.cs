using System.Threading.Tasks;
using Blazorise;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Dialogs;

public partial class ConfirmDeleteUserDialog
{
    EmailAddress email;
    string? fullName;
    Modal modal = null!;
    Button submitButton = null!;

    [Parameter] public EventCallback OnConfirm { get; set; }

    public ValueTask ShowAsync(User user)
    {
        email = user.Information.Email;
        fullName = user.Information.FullName;
        modal.Show();
        return submitButton.ElementRef.FocusAsync();
    }

    Task Confirm()
    {
        modal.Hide();
        return OnConfirm.InvokeAsync(null);
    }

    void Cancel() => modal.Hide();
}
