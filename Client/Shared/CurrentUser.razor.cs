using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class CurrentUser
{
    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public AuthenticationService AuthenticationService { get; set; } = null!;
    [Inject] public SignOutService SignOutService { get; set; } = null!;

    async Task SignOutAsync()
    {
        await ApiClient.PostAsync("user/sign-out");
        await SignOutService.SignOutAsync();
    }
}
