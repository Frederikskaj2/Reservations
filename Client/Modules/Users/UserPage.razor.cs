using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

[Authorize(Policy = Policy.ViewUsers)]
partial class UserPage
{
    bool canEditUserInformation;
    UserId currentUserId;
    bool isInitialized;
    bool showUserUpdatedAlert;
    bool showUserUpdatedErrorAlert;
    UserDetailsDto? user;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public TokenAuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    [Parameter] public int UserId { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        currentUserId = state.User.UserId();
        canEditUserInformation = state.User.IsInRole(nameof(Roles.UserAdministration));
        var response = await ApiClient.Get<GetUserResponse>($"users/{UserId}");
        user = response.Result!.User;
        isInitialized = true;
    }

    async Task UpdateUser(UpdateUserRequest request)
    {
        DismissAllAlerts();
        var response = await ApiClient.Patch<UpdateUserResponse>($"users/{user!.Identity.UserId}", request);
        if (response.IsSuccess)
        {
            user = response.Result!.User;
            showUserUpdatedAlert = true;
        }
        else
            showUserUpdatedErrorAlert = true;
    }

    async Task DeleteUser()
    {
        DismissAllAlerts();
        var request = new UpdateUserRequest(user!.Identity.FullName, user.Identity.Phone, user.Roles, IsPendingDelete: true);
        var response = await ApiClient.Patch<UpdateUserResponse>($"users/{user!.Identity.UserId}", request);
        if (response.IsSuccess)
        {
            user = response.Result!.User;
            showUserUpdatedAlert = true;
        }
        else
            showUserUpdatedErrorAlert = true;
    }

    void DismissUserUpdatedAlert() => showUserUpdatedAlert = false;

    void DismissUserUpdatedErrorAlert() => showUserUpdatedErrorAlert = false;

    void DismissAllAlerts()
    {
        showUserUpdatedAlert = false;
        showUserUpdatedErrorAlert = false;
    }
}
