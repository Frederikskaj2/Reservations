using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize]
public partial class MyUserPage
{
    bool isInitialized;
    MyUser? user;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var response = await ApiClient.GetAsync<MyUser>("user");
        user = response.Result;
        isInitialized = true;
    }
}
