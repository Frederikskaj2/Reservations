using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client;

public class RedirectState(NavigationManager navigationManager)
{
    public string? RedirectUrl { get; set; }

    public void Redirect()
    {
        var redirectUrl = RedirectUrl;
        RedirectUrl = null;
        navigationManager.NavigateTo(redirectUrl ?? "");
    }
}
