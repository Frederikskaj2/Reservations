using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class MyUserDelete
{
    ConfirmDeleteMyUserDialog confirmDeleteDialog = null!;
    bool showErrorAlert;
    bool showPendingDeleteAlert;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public SignOutService SignOutService { get; set; } = null!;

    [Parameter] public GetMyUserResponse User { get; set; } = null!;

    Task ConfirmDeleteUser()
    {
        DismissAllAlerts();
        return confirmDeleteDialog.Show();
    }

    async Task DeleteUser()
    {
        var response = await ApiClient.Delete<DeleteUserResponse>("user");
        if (!response.IsSuccess)
        {
            showErrorAlert = true;
            return;
        }

        if (response.Result!.Status is DeleteUserStatus.Pending)
        {
            showPendingDeleteAlert = true;
            User = User with { IsPendingDelete = true };
            return;
        }

        await SignOutService.SignOut();
    }

    void DismissErrorAlert() => showErrorAlert = false;

    void DismissPendingDeleteAlert() => showPendingDeleteAlert = false;

    void DismissAllAlerts()
    {
        showErrorAlert = false;
        showPendingDeleteAlert = false;
    }
}
