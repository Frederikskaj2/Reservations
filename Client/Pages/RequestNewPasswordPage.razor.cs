using System.Threading.Tasks;
using Blazorise;
using Frederikskaj2.Reservations.Client.Editors;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Pages;

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
            await emailEditor.FocusAsync();
    }

    async Task SendEmailAsync()
    {
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        DismissSuccessAlert();
        DismissErrorAlert();
        isDisabled = true;

        var request = new SendNewPasswordEmailRequest { Email = SignInState.Email };
        var response = await ApiClient.PostAsync("user/send-new-password-email", request);
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