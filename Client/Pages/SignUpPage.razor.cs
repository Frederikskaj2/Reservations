using Blazorise;
using Frederikskaj2.Reservations.Client.Editors;
using Frederikskaj2.Reservations.Client.Shared;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

public partial class SignUpPage
{
    EmailEditor emailEditor = null!;
    SignUpError error;
    bool isDisabled;
    bool isInitialized;
    IndefiniteProgressBar progressBar = null!;
    bool showErrorAlert;
    bool showSuccessAlert;
    Validations validations = null!;

    [Inject] public ApiClient ApiClient { get; set; } = null!;
    [Inject] public SignUpState SignUpState { get; set; } = null!;

    protected override void OnInitialized()
    {
        SignUpState.ViewModel.Password = null;
        isInitialized = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await emailEditor.FocusAsync();
    }

    void ValidateConfirmPassword(ValidatorEventArgs e) =>
        e.Status = e.Value?.ToString() == SignUpState.ViewModel.Password ? ValidationStatus.Success : ValidationStatus.Error;

    async Task SubmitAsync()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        DismissErrorAlert();

        isDisabled = true;
        progressBar.Start();
        var response = await ApiClient.PostAsync("user/sign-up", SignUpState.ViewModel);
        progressBar.Stop();

        if (response.IsSuccess)
        {
            showSuccessAlert = true;
            return;
        }

        error = response.Problem!.GetError<SignUpError>();
        isDisabled = false;
        showErrorAlert = true;
    }

    void DismissErrorAlert() => (showSuccessAlert, showErrorAlert) = (false, false);
}
