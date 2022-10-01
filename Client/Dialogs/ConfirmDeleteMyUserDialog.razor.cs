using Blazorise;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Dialogs;

public partial class ConfirmDeleteMyUserDialog
{
    Modal modal = null!;
    Button submitButton = null!;

    [Parameter] public EventCallback OnConfirm { get; set; }

    public ValueTask ShowAsync()
    {
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