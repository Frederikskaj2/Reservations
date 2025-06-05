using Blazorise;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class MyUserInformation
{
    string fullName = "";
    string phone = "";
    bool showResendConfirmEmailEmailAlert;
    bool showResendConfirmEmailEmailErrorAlert;
    bool showUserUpdatedAlert;
    bool showUserUpdatedErrorAlert;
    GetMyUserResponse user = null!;
    Validations validations = null!;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;

    [Parameter] public GetMyUserResponse User { get; set; } = null!;

    protected override void OnParametersSet() => SetUser(User);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await validations.ClearAll();
    }

    async Task UpdateMyUser()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        DismissAllAlerts();

        var request = new UpdateMyUserRequest(fullName, phone, EmailSubscriptions.None);
        var response = await ApiClient.Patch<GetMyUserResponse>("user", request);
        if (response.Result is not null)
        {
            SetUser(response.Result);
            showUserUpdatedAlert = true;
        }
        else
            showUserUpdatedErrorAlert = true;
    }

    async Task SendConfirmEmailEmail()
    {
        DismissAllAlerts();
        var response = await ApiClient.Post("user/resend-confirm-email-email");
        if (response.IsSuccess)
            showResendConfirmEmailEmailAlert = true;
        else
            showResendConfirmEmailEmailErrorAlert = true;
    }

    void SetUser(GetMyUserResponse getMyUserResponse)
    {
        user = getMyUserResponse;
        fullName = getMyUserResponse.Identity.FullName;
        phone = getMyUserResponse.Identity.Phone;
    }

    void DismissUserUpdatedAlert() => showUserUpdatedAlert = false;
    void DismissUserUpdatedErrorAlert() => showUserUpdatedErrorAlert = false;
    void DismissResendConfirmEmailEmailAlert() => showResendConfirmEmailEmailAlert = false;
    void DismissResendConfirmEmailEmailErrorAlert() => showResendConfirmEmailEmailErrorAlert = false;

    void DismissAllAlerts()
    {
        showUserUpdatedAlert = false;
        showUserUpdatedErrorAlert = false;
        showResendConfirmEmailEmailAlert = false;
        showResendConfirmEmailEmailErrorAlert = false;
    }
}
