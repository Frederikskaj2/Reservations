using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

[Authorize]
partial class MyUserPage
{
    bool isInitialized;
    GetMyUserResponse? user;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var response = await ApiClient.Get<GetMyUserResponse>("user");
        user = response.Result;
        isInitialized = true;
    }
}
