using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client;

public class SignOutService(AuthenticationService authenticationService, EventAggregator eventAggregator, NavigationManager navigationManager)
{
    public async ValueTask SignOut()
    {
        await authenticationService.Clear();
        eventAggregator.Publish(SignOutMessage.Instance);
        navigationManager.NavigateTo("");
    }
}
