using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class SignInOrSignUp
{
    [Inject] public RedirectState RedirectState { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    void SignIn()
    {
        RedirectState.RedirectUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        NavigationManager.NavigateTo(Frederikskaj2.Reservations.Core.UrlPath.SignIn);
    }

    void SignUp()
    {
        RedirectState.RedirectUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        NavigationManager.NavigateTo(Frederikskaj2.Reservations.Core.UrlPath.SignUp);
    }
}
