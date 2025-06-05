using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

public sealed partial class UserRoles
{
    bool isDisabled;
    bool isUserAdministrationDisabled;
    Roles roles;

    [Inject] public TokenAuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Parameter] public bool IsReadOnly { get; set; }
    [Parameter] public EventCallback<UpdateUserRequest> OnUpdate { get; set; }
    [Parameter] public UserDetailsDto User { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var isCurrentUser = state.User.UserId() == User.Identity.UserId;
        isDisabled = IsReadOnly || User.IsDeleted;
        isUserAdministrationDisabled = isDisabled || isCurrentUser;
        roles = User.Roles;
    }

    Task UpdateUser()
    {
        var request = new UpdateUserRequest(User.Identity.FullName, User.Identity.Phone, roles, User.IsPendingDelete);
        return OnUpdate.InvokeAsync(request);
    }

    void CheckRole(bool isChecked, Roles role)
    {
        if (isChecked)
            roles |= role;
        else
            roles &= ~role;
    }
}
