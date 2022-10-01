using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Shared;

public sealed partial class UserRoles
{
    bool isDisabled;
    bool isUserAdministrationDisabled;
    Roles roles;

    [Inject] public TokenAuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Parameter] public bool IsReadOnly { get; set; }
    [Parameter] public EventCallback<UpdateUserRequest> OnUpdate { get; set; }
    [Parameter] public UserDetails User { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var isCurrentUser = state.User.UserId() == User.Information.UserId;
        isDisabled = IsReadOnly || User.IsDeleted;
        isUserAdministrationDisabled = isDisabled || isCurrentUser;
        roles = User.Roles;
    }

    Task UpdateUserAsync()
    {
        var request = new UpdateUserRequest
        {
            FullName = User.Information.FullName,
            Phone = User.Information.Phone,
            Roles = roles,
            IsPendingDelete = User.IsPendingDelete
        };
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
