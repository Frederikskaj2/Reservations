using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Policy = Policies.ViewUsers)]
public partial class UserPage
{
    bool canEditUserInformation;
    UserId currentUserId;
    bool isInitialized;
    bool showUserUpdatedAlert;
    bool showUserUpdatedErrorAlert;
    UserDetails? user;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public TokenAuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    [Parameter] public int UserId { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        currentUserId = state.User.UserId();
        canEditUserInformation = state.User.IsInRole(nameof(Roles.UserAdministration));
        var response = await ApiClient.GetAsync<UserDetails>($"users/{UserId}");
        user = response.Result;
        isInitialized = true;
    }

    async Task UpdateUserAsync(UpdateUserRequest request)
    {
        DismissAllAlerts();
        var response = await ApiClient.PatchAsync<UpdateUserResponse>($"users/{user!.Information.UserId}", request);
        if (response.IsSuccess)
        {
            if (response.Result!.Result == UpdateUserResult.UserWasDeleted)
                NavigationManager.NavigateTo(Urls.Users);
            else
            {
                user = response.Result!.User;
                showUserUpdatedAlert = true;
            }
        }
        else
            showUserUpdatedErrorAlert = true;
    }

    async Task DeleteUserAsync()
    {
        DismissAllAlerts();
        var request = new UpdateUserRequest
        {
            FullName = user!.Information.FullName,
            Phone = user.Information.Phone,
            Roles = user.Roles,
            IsPendingDelete = true
        };
        var response = await ApiClient.PatchAsync<UpdateUserResponse>($"users/{user!.Information.UserId}", request);
        if (response.IsSuccess)
        {
            if (response.Result!.Result == UpdateUserResult.UserWasDeleted)
                NavigationManager.NavigateTo(Urls.Users);
            else
            {
                user = response.Result!.User;
                showUserUpdatedAlert = true;
            }
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
