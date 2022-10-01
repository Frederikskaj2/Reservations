using System.Threading.Tasks;
using Frederikskaj2.Reservations.Client.Dialogs;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class UserDelete
{
    ConfirmDeleteUserDialog confirmDeleteDialog = null!;

    [Parameter] public EventCallback OnDelete { get; set; }
    [Parameter] public UserDetails User { get; set; } = null!;
    [Parameter] public bool IsCurrentUser { get; set; }

    async Task ConfirmDeleteAsync() => await confirmDeleteDialog.ShowAsync(User);

    async Task DeleteAsync() => await OnDelete.InvokeAsync();
}
