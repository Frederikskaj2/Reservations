﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;

namespace Frederikskaj2.Reservations.Client.Pages;

public partial class ConfirmEmailPage
{
    string? email;
    ConfirmEmailError error;
    bool isInitialized;
    bool showConfirmErrorAlert;
    bool showResendErrorAlert;
    bool showResendSuccessAlert;

    [Inject] public ApiClient ApiClient { get; set; } = null!;
    [Inject] public AuthenticatedApiClient AuthenticatedApiClient { get; set; } = null!;
    [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var query = QueryParser.GetQuery(NavigationManager.Uri);
        string? emailFromQuery = null;
        if (query.Contains("email") && query.Contains("token"))
        {
            emailFromQuery = query["email"].FirstOrDefault();
            var request = new ConfirmEmailRequest { Email = emailFromQuery, Token = query["token"].FirstOrDefault() };
            var response = await ApiClient.PostAsync("user/confirm-email", request);
            if (!response.IsSuccess)
            {
                error = response.Problem!.GetError<ConfirmEmailError>();
                showConfirmErrorAlert = true;
            }
        }
        else
            showConfirmErrorAlert = true;

        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        email = authenticationState.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value ?? emailFromQuery;
        isInitialized = true;
    }

    async Task ResendConfirmEmail()
    {
        DismissResendAlerts();
        var response = await AuthenticatedApiClient.PostAsync("user/resend-confirm-email-email");
        if (response.IsSuccess)
            showResendSuccessAlert = true;
        else
            showResendErrorAlert = true;
    }

    void DismissResendAlerts()
    {
        showResendErrorAlert = false;
        showResendSuccessAlert = false;
    }
}
