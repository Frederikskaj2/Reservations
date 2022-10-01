using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client;

public class SignOutService
{
    readonly AuthenticationService authenticationService;
    readonly EventAggregator eventAggregator;
    readonly NavigationManager navigationManager;

    public SignOutService(AuthenticationService authenticationService, EventAggregator eventAggregator, NavigationManager navigationManager)
    {
        this.authenticationService = authenticationService;
        this.eventAggregator = eventAggregator;
        this.navigationManager = navigationManager;
    }

    public async ValueTask SignOutAsync()
    {
        await authenticationService.ClearAsync();
        eventAggregator.Publish(SignOutMessage.Instance);
        navigationManager.NavigateTo("");
    }
}