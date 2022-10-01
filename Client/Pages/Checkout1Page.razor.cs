using Blazorise;
using Frederikskaj2.Reservations.Client.Editors;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.Resident))]
public partial class Checkout1Page
{
    FullNameEditor fullNameEditor = null!;
    bool hasConfirmedEmail;
    bool isInitialized;
    bool showEmailNotConfirmedAlert;
    bool showResendConfirmEmailEmailAlert;
    bool showResendConfirmEmailEmailErrorAlert;
    Validations validations = null!;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public DraftOrder DraftOrder { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public UserOrderInformation OrderInformation { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var response = await ApiClient.GetAsync<MyUser>("user");
        if (response.IsSuccess)
        {
            hasConfirmedEmail = response.Result!.IsEmailConfirmed;
            showEmailNotConfirmedAlert = !hasConfirmedEmail;
            if (string.IsNullOrEmpty(OrderInformation.FullName))
                OrderInformation.FullName = response.Result!.Information.FullName;
            if (string.IsNullOrEmpty(OrderInformation.Phone))
                OrderInformation.Phone = response.Result!.Information.Phone;
            if (!OrderInformation.ApartmentId.HasValue)
                OrderInformation.ApartmentId = response.Result!.Information.ApartmentId;
            if (string.IsNullOrEmpty(OrderInformation.AccountNumber))
                OrderInformation.AccountNumber = response.Result!.AccountNumber;
        }
        isInitialized = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && hasConfirmedEmail)
            await fullNameEditor.FocusAsync();
    }

    async Task ResendConfirmEmail()
    {
        DismissAllAlerts();
        var response = await ApiClient.PostAsync("user/resend-confirm-email-email");
        if (!response.IsSuccess)
            showResendConfirmEmailEmailErrorAlert = true;
        else
            showResendConfirmEmailEmailAlert = true;
    }

    async Task UpdateEmailConfirmationStatus()
    {
        DismissAllAlerts();
        var response = await ApiClient.GetAsync<MyUser>("user");
        hasConfirmedEmail = response.IsSuccess && response.Result!.IsEmailConfirmed;
        showEmailNotConfirmedAlert = !hasConfirmedEmail;
    }

    async Task SubmitAsync()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        NavigationManager.NavigateTo(Urls.Checkout2);
    }

    void DismissResendConfirmEmailEmailErrorAlert() => showResendConfirmEmailEmailErrorAlert = false;

    void DismissAllAlerts()
    {
        showEmailNotConfirmedAlert = false;
        showResendConfirmEmailEmailAlert = false;
        showResendConfirmEmailEmailErrorAlert = false;
    }
}
