using Frederikskaj2.Reservations.Client.Dialogs;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class MyUserDelete
{
    ConfirmDeleteMyUserDialog confirmDeleteDialog = null!;
    bool showErrorAlert;
    bool showPendingDeleteAlert;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public SignOutService SignOutService { get; set; } = null!;

    [Parameter] public MyUser User { get; set; } = null!;

    async Task ConfirmDeleteUserAsync()
    {
        DismissAllAlerts();
        await confirmDeleteDialog.ShowAsync();
    }

    async Task DeleteUserAsync()
    {
        var response = await ApiClient.DeleteAsync<DeleteUserResponse>("user");
        if (!response.IsSuccess)
        {
            showErrorAlert = true;
            return;
        }

        if (response.Result!.Result == DeleteUserResult.IsPendingDelete)
        {
            showPendingDeleteAlert = true;
            User = User with { IsPendingDelete = true };
            return;
        }

        await SignOutService.SignOutAsync();
    }

    void DismissErrorAlert() => showErrorAlert = false;

    void DismissPendingDeleteAlert() => showPendingDeleteAlert = false;

    void DismissAllAlerts()
    {
        showErrorAlert = false;
        showPendingDeleteAlert = false;
    }
}
