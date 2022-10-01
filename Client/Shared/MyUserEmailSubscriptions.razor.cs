using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class MyUserEmailSubscriptions
{
    readonly UpdateMyUserRequest request = new();
    bool showErrorAlert;
    bool showSuccessAlert;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;

    [Parameter] public MyUser User { get; set; } = null!;

    protected override void OnParametersSet()
    {
        request.FullName = User.Information.FullName!;
        request.Phone = User.Information.Phone!;
        request.EmailSubscriptions = User.EmailSubscriptions;
    }

    void CheckSubscription(bool isChecked, EmailSubscriptions subscription)
    {
        if (isChecked)
            request.EmailSubscriptions |= subscription;
        else
            request.EmailSubscriptions &= ~subscription;
    }

    async Task UpdateMyUser()
    {
        DismissAllAlerts();

        var response = await ApiClient.PatchAsync("user", request);
        if (response.IsSuccess)
            showSuccessAlert = true;
        else
            showErrorAlert = true;
    }

    void DismissSuccessAlert() => showSuccessAlert = false;
    void DismissErrorAlert() => showErrorAlert = false;

    void DismissAllAlerts()
    {
        showSuccessAlert = false;
        showErrorAlert = false;
    }
}
