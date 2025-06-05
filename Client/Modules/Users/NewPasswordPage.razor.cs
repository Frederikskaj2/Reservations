using Blazorise;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

public partial class NewPasswordPage
{
    string? email;
    NewPasswordError error;
    bool isDisabled;
    bool isInitialized;
    string? newPassword;
    bool showErrorAlert;
    bool showLinkAlert;
    bool showSuccessAlert;
    bool showSendMailErrorAlert;
    bool showSendMailSuccessAlert;
    string? token;
    Validations validations = null!;

    [Inject] public ApiClient ApiClient { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public SignInState SignInState { get; set; } = null!;

    protected override void OnInitialized()
    {
        var query = QueryParser.GetQuery(NavigationManager.Uri);
        if (query.Contains("email") && query.Contains("token"))
        {
            email = query["email"].FirstOrDefault();
            token = query["token"].FirstOrDefault();
            SignInState.Email = email;
        }
        else
            showLinkAlert = true;

        isInitialized = true;
    }

    async Task Submit()
    {
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        DismissAllAlerts();
        isDisabled = true;

        var request = new NewPasswordRequest(email, token, newPassword);
        var response = await ApiClient.Post("user/new-password", request);
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
        var response = await ApiClient.Post("user/send-new-password-email", new SendNewPasswordEmailRequest(Email: SignInState.Email));
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

    void ValidateConfirmNewPassword(ValidatorEventArgs eventArgs) =>
        eventArgs.Status = eventArgs.Value?.ToString() == newPassword ? ValidationStatus.Success : ValidationStatus.Error;
}
