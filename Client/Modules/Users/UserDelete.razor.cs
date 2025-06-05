using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class UserDelete
{
    ConfirmDeleteUserDialog confirmDeleteDialog = null!;

    [Parameter] public EventCallback OnDelete { get; set; }
    [Parameter] public UserDetailsDto User { get; set; } = null!;
    [Parameter] public bool IsCurrentUser { get; set; }

    async Task ConfirmDelete() => await confirmDeleteDialog.Show(User);

    Task Delete() => OnDelete.InvokeAsync();
}
