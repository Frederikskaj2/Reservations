﻿using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class CurrentUser
{
    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public DraftOrder DraftOrder { get; set; } = null!;
    [Inject] public SignOutService SignOutService { get; set; } = null!;
    [Inject] public UserOrderInformation OrderInformation { get; set; } = null!;

    async Task SignOut()
    {
        DraftOrder.Clear();
        OrderInformation.Clear();
        await ApiClient.Post("user/sign-out");
        await SignOutService.SignOut();
    }
}
