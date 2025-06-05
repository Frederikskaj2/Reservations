using Blazorise;
using Frederikskaj2.Reservations.Client.Modules.Core;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class SignInPage
{
    EmailEditor emailEditor = null!;
    bool isDisabled;
    bool isPersistent;
    string? password;
    IndefiniteProgressBar progressBar = null!;
    bool showErrorAlert;
    SignInError? signInError;
    Validations validations = null!;

    [Inject] public ApiClient ApiClient { get; set; } = null!;
    [Inject] public AuthenticationService AuthenticationService { get; set; } = null!;
    [Inject] public EventAggregator EventAggregator { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public RedirectState RedirectState { get; set; } = null!;
    [Inject] public SignInState SignInState { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await emailEditor.Focus();
    }

    async Task Submit()
    {
        if (!await validations.ValidateAll())
            return;

        await validations.ClearAll();
        DismissErrorAlert();

        isDisabled = true;
        var response = await SignIn();
        if (response.IsSuccess)
        {
            EventAggregator.Publish(SignInMessage.Instance);
            RedirectState.Redirect();
            return;
        }

        signInError = response.Problem!.GetError<SignInError>();
        isDisabled = false;
        showErrorAlert = true;
    }

    async Task<ApiResponse<Tokens>> SignIn()
    {
        var request = new SignInRequest(Email: SignInState.Email, Password: password, IsPersistent: isPersistent);
        progressBar.Start();
        var response = await ApiClient.Post<Tokens>("user/sign-in", request);
        progressBar.Stop();
        if (response.IsSuccess)
            await AuthenticationService.SetTokens(response.Result!);
        return response;
    }

    void DismissErrorAlert() => showErrorAlert = false;

    void SignUp() => NavigationManager.NavigateTo(Frederikskaj2.Reservations.Core.UrlPath.SignUp);
}
