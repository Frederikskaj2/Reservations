using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class MyUserEmailSubscriptions
{
    EmailSubscriptions emailSubscriptions;
    string? fullName;
    string? phone;
    bool showErrorAlert;
    bool showSuccessAlert;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;

    [Parameter] public GetMyUserResponse User { get; set; } = null!;

    protected override void OnParametersSet()
    {
        fullName = User.Identity.FullName;
        phone = User.Identity.Phone;
        emailSubscriptions = User.EmailSubscriptions;
    }

    void CheckSubscription(bool isChecked, EmailSubscriptions subscription)
    {
        if (isChecked)
            emailSubscriptions |= subscription;
        else
            emailSubscriptions &= ~subscription;
    }

    async Task UpdateMyUser()
    {
        DismissAllAlerts();

        var request = new UpdateMyUserRequest(fullName, phone, emailSubscriptions);
        var response = await ApiClient.Patch("user", request);
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
