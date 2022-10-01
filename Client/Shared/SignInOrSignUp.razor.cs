using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class SignInOrSignUp
{
    [Inject] public RedirectState RedirectState { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    void SignIn()
    {
        RedirectState.RedirectUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        NavigationManager.NavigateTo(Urls.SignIn);
    }

    void SignUp()
    {
        RedirectState.RedirectUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        NavigationManager.NavigateTo(Urls.SignUp);
    }
}
