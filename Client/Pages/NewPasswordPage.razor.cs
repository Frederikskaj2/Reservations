using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Pages;

public partial class NewPasswordPage
{
    readonly NewPasswordRequest request = new();
    NewPasswordError error;
    bool isDisabled;
    bool isInitialized;
    bool showErrorAlert;
    bool showLinkAlert;
    bool showSuccessAlert;
    bool showSendMailErrorAlert;
    bool showSendMailSuccessAlert;
    Validations validations = null!;

    [Inject] public ApiClient ApiClient { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public SignInState SignInState { get; set; } = null!;

    protected override void OnInitialized()
    {
        var query = QueryParser.GetQuery(NavigationManager.Uri);
        if (query.Contains("email") && query.Contains("token"))
        {
            request.Email = query["email"].FirstOrDefault();
            request.Token = query["token"].FirstOrDefault();
            SignInState.Email = request.Email;
        }
        else
            showLinkAlert = true;

        isInitialized = true;
    }

    async Task SubmitAsync()
    {
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        DismissAllAlerts();
        isDisabled = true;

        var response = await ApiClient.PostAsync("user/new-password", request);
        if (response.IsSuccess)
            showSuccessAlert = true;
        else
        {
            error = response.Problem!.GetError<NewPasswordError>();
            showErrorAlert = true;
        }
    }

    async Task SendEmail()
    {
        var response = await ApiClient.PostAsync("user/send-new-password-email", new SendNewPasswordEmailRequest { Email = SignInState.Email });
        if (response.IsSuccess)
            showSendMailSuccessAlert = true;
        else
            showSendMailErrorAlert = true;
    }


    void DismissErrorAlert()
    {
        showErrorAlert = false;
        isDisabled = false;
    }

    void DismissSendMailAlerts()
    {
        showSendMailSuccessAlert = false;
        showSendMailErrorAlert = false;
    }

    void DismissAllAlerts()
    {
        showErrorAlert = false;
        showSendMailSuccessAlert = false;
        showSendMailErrorAlert = false;
    }

    void ValidateConfirmNewPassword(ValidatorEventArgs e) =>
        e.Status = e.Value?.ToString() == request.NewPassword ? ValidationStatus.Success : ValidationStatus.Error;
}
