using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.LockBox;

[Authorize(Roles = nameof(Roles.LockBoxCodes))]
public partial class LockBoxCodesPage
{
    IEnumerable<WeeklyLockBoxCodesDto>? allWeeklyLockBoxCodes;
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
        resources = await ClientDataProvider.GetResources();
        var response = await ApiClient.Get<GetLockBoxCodesResponse>("lock-box-codes");
        allWeeklyLockBoxCodes = response.Result!.LockBoxCodes;
        isInitialized = true;
    }

    async Task SendList()
    {
        DismissAllAlerts();

        var response = await ApiClient.Post<SendLockBoxCodesResponse>("lock-box-codes/send");
        if (response.IsSuccess)
        {
            email = response.Result!.Email;
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
