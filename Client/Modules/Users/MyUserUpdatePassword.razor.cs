using Blazorise;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class MyUserUpdatePassword
{
    string? confirmPassword;
    string? currentPassword;
    PasswordError error;
    string? newPassword;
    bool showPasswordUpdatedAlert;
    bool showPasswordUpdatedErrorAlert;
    bool showSignedOutEverywhereAlert;
    bool showSignedOutEverywhereErrorAlert;
    Validations validations = null!;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public AuthenticationService AuthenticationService { get; set; } = null!;

    [Parameter] public GetMyUserResponse User { get; set; } = null!;

    void ValidateConfirmNewPassword(ValidatorEventArgs e) =>
        e.Status = e.Value?.ToString() == newPassword ? ValidationStatus.Success : ValidationStatus.Error;

    async Task Submit()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        DismissAllAlerts();

        var response = await UpdatePassword();
        if (response.IsSuccess)
        {
            showPasswordUpdatedAlert = true;
            currentPassword = null;
            newPassword = null;
            confirmPassword = null;
            return;
        }

        error = response.Problem!.GetError<PasswordError>();
        showPasswordUpdatedErrorAlert = true;
    }

    async Task<ApiResponse<Tokens>> UpdatePassword()
    {
        var request = new UpdatePasswordRequest(CurrentPassword: currentPassword, NewPassword: newPassword);
        var response = await ApiClient.Post<Tokens>("user/update-password", request);
        if (response.IsSuccess)
            await AuthenticationService.SetTokens(response.Result!);
        return response;
    }

    async Task SignOutEverywhereElse()
    {
        DismissAllAlerts();
        var isSuccess = await SignOutEverywhereElseCore();
        if (isSuccess)
            showSignedOutEverywhereAlert = true;
        else
            showSignedOutEverywhereErrorAlert = true;
    }

    async Task<bool> SignOutEverywhereElseCore()
    {
        var response = await ApiClient.Post<Tokens>("user/sign-out-everywhere-else");
        if (!response.IsSuccess)
            return false;
        await AuthenticationService.SetTokens(response.Result!);
        return true;
    }

    void DismissPasswordUpdatedAlert() => showPasswordUpdatedAlert = false;
    void DismissPasswordUpdatedErrorAlert() => showPasswordUpdatedErrorAlert = false;
    void DismissSignedOutEverywhereAlert() => showSignedOutEverywhereAlert = false;
    void DismissSignedOutEverywhereErrorAlert() => showSignedOutEverywhereErrorAlert = false;

    void DismissAllAlerts()
    {
        showPasswordUpdatedAlert = false;
        showPasswordUpdatedErrorAlert = false;
        showSignedOutEverywhereAlert = false;
        showSignedOutEverywhereErrorAlert = false;
    }
}
