using Blazorise;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class MyUserUpdatePassword
{
    readonly UpdatePasswordRequest request = new();
    string? confirmPassword;
    PasswordError error;
    bool showPasswordUpdatedAlert;
    bool showPasswordUpdatedErrorAlert;
    bool showSignedOutEverywhereAlert;
    bool showSignedOutEverywhereErrorAlert;
    Validations validations = null!;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public AuthenticationService AuthenticationService { get; set; } = null!;

    [Parameter] public MyUser User { get; set; } = null!;

    void ValidateConfirmNewPassword(ValidatorEventArgs e) =>
        e.Status = e.Value?.ToString() == request.NewPassword ? ValidationStatus.Success : ValidationStatus.Error;

    async Task SubmitAsync()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        DismissAllAlerts();

        var response = await UpdatePasswordAsync();
        if (response.IsSuccess)
        {
            showPasswordUpdatedAlert = true;
            request.CurrentPassword = null;
            request.NewPassword = null;
            confirmPassword = null;
            return;
        }

        error = response.Problem!.GetError<PasswordError>();
        showPasswordUpdatedErrorAlert = true;
    }

    async Task<ApiResponse<Tokens>> UpdatePasswordAsync()
    {
        var response = await ApiClient.PostAsync<Tokens>("user/update-password", request);
        if (response.IsSuccess)
            await AuthenticationService.SetTokensAsync(response.Result!);
        return response;
    }

    async Task SignOutEverywhereElseAsync()
    {
        DismissAllAlerts();
        var isSuccess = await SignOutEverywhereElseCoreAsync();
        if (isSuccess)
            showSignedOutEverywhereAlert = true;
        else
            showSignedOutEverywhereErrorAlert = true;
    }

    async Task<bool> SignOutEverywhereElseCoreAsync()
    {
        var response = await ApiClient.PostAsync<Tokens>("user/sign-out-everywhere-else");
        if (!response.IsSuccess)
            return false;
        await AuthenticationService.SetTokensAsync(response.Result!);
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
