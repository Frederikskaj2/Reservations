using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.LockBoxCodes))]
public partial class LockBoxCodesPage
{
    IEnumerable<WeeklyLockBoxCodes>? allWeeklyLockBoxCodes;
    EmailAddress email;
    bool isInitialized;
    IReadOnlyDictionary<ResourceId, Resource>? resources;
    bool showErrorAlert;
    bool showSuccessAlert;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        resources = await ClientDataProvider.GetResourcesAsync();
        var response = await ApiClient.GetAsync<IEnumerable<WeeklyLockBoxCodes>>("lock-box-codes");
        allWeeklyLockBoxCodes = response.Result;
        isInitialized = true;
    }

    async Task SendList()
    {
        DismissAllAlerts();

        var response = await ApiClient.PostAsync<EmailAddress>("lock-box-codes/send");
        if (response.IsSuccess)
        {
            email = response.Result;
            showSuccessAlert = true;
        }
        else
            showErrorAlert = true;
    }

    void DismissSuccessAlert() => showSuccessAlert = false;

    void DismissErrorAlert() => showErrorAlert = false;

    void DismissAllAlerts()
    {
        DismissSuccessAlert();
        DismissErrorAlert();
    }
}
