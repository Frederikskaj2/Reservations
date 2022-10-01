using Blazorise;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class MyUserInformation
{
    readonly UpdateMyUserRequest request = new();
    bool showResendConfirmEmailEmailAlert;
    bool showResendConfirmEmailEmailErrorAlert;
    bool showUserUpdatedAlert;
    bool showUserUpdatedErrorAlert;
    MyUser user = null!;
    Validations validations = null!;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;

    [Parameter] public MyUser User { get; set; } = null!;

    protected override void OnParametersSet() => SetUser(User);

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            validations.ClearAll();
    }

    async Task UpdateMyUserAsync()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        DismissAllAlerts();

        var response = await ApiClient.PatchAsync<MyUser>("user", request);
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
        var response = await ApiClient.PostAsync("user/resend-confirm-email-email");
        if (response.IsSuccess)
            showResendConfirmEmailEmailAlert = true;
        else
            showResendConfirmEmailEmailErrorAlert = true;
    }

    void SetUser(MyUser myUser)
    {
        user = myUser;
        request.FullName = user.Information.FullName!;
        request.Phone = user.Information.Phone!;
        request.EmailSubscriptions = user.EmailSubscriptions;
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
