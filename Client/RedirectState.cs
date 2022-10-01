using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client;

public class RedirectState
{
    readonly NavigationManager navigationManager;

    public RedirectState(NavigationManager navigationManager) => this.navigationManager = navigationManager;

    public string? RedirectUrl { get; set; }

    public void Redirect()
    {
        var redirectUrl = RedirectUrl;
        RedirectUrl = null;
        navigationManager.NavigateTo(redirectUrl ?? "");
    }
}