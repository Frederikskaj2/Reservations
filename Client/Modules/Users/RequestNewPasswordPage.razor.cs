using Blazorise;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

public partial class RequestNewPasswordPage
{
    EmailEditor emailEditor = null!;
    bool isDisabled;
    bool showErrorAlert;
    bool showSuccessAlert;
    Validations validations = null!;

    [Inject] public ApiClient ApiClient { get; set; } = null!;
    [Inject] public SignInState SignInState { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await emailEditor.Focus();
    }

    async Task SendEmail()
    {
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        DismissSuccessAlert();
        DismissErrorAlert();
        isDisabled = true;

        var request = new SendNewPasswordEmailRequest(Email: SignInState.Email);
        var response = await ApiClient.Post("user/send-new-password-email", request);
        if (response.IsSuccess)
            showSuccessAlert = true;
        else
        {
            showErrorAlert = true;
            isDisabled = false;
        }
    }

    void DismissSuccessAlert()
    {
        showSuccessAlert = false;
        isDisabled = false;
    }

    void DismissErrorAlert() => showErrorAlert = false;
}
