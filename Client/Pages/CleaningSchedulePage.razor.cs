using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.Cleaning))]
public partial class CleaningSchedulePage
{
    const string calendar = nameof(calendar);
    const string list = nameof(list);
    IEnumerable<CleaningTask>? cleaningTasks;
    string currentDisplay = calendar;
    EmailAddress email;
    bool isInitialized;
    IEnumerable<ReservedDay>? reservedDays;
    IReadOnlyDictionary<ResourceId, Resource>? resources;
    bool showSuccessAlert;
    bool showErrorAlert;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        resources = await ClientDataProvider.GetResourcesAsync();
        var response = await ApiClient.GetAsync<CleaningSchedule>("cleaning-schedule");
        cleaningTasks = response.Result!.CleaningTasks;
        reservedDays = response.Result.ReservedDays;
        isInitialized = true;
    }

    async Task SendList()
    {
        DismissAllAlerts();

        var response = await ApiClient.PostAsync<EmailAddress>("cleaning-tasks/send");
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
