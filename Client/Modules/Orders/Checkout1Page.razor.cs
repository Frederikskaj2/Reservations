using Blazorise;
using Frederikskaj2.Reservations.Client.Modules.Users;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

[Authorize(Roles = nameof(Roles.Resident))]
partial class Checkout1Page
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
        var response = await ApiClient.Get<GetMyUserResponse>("user");
        if (response.IsSuccess)
        {
            hasConfirmedEmail = response.Result!.IsEmailConfirmed;
            showEmailNotConfirmedAlert = !hasConfirmedEmail;
            if (string.IsNullOrEmpty(OrderInformation.FullName))
                OrderInformation.FullName = response.Result!.Identity.FullName;
            if (string.IsNullOrEmpty(OrderInformation.Phone))
                OrderInformation.Phone = response.Result!.Identity.Phone;
            OrderInformation.ApartmentId ??= response.Result!.Identity.ApartmentId;
            if (string.IsNullOrEmpty(OrderInformation.AccountNumber))
                OrderInformation.AccountNumber = response.Result!.AccountNumber;
        }
        isInitialized = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && hasConfirmedEmail)
            await fullNameEditor.Focus();
    }

    async Task ResendConfirmEmail()
    {
        DismissAllAlerts();
        var response = await ApiClient.Post("user/resend-confirm-email-email");
        if (!response.IsSuccess)
            showResendConfirmEmailEmailErrorAlert = true;
        else
            showResendConfirmEmailEmailAlert = true;
    }

    async Task UpdateEmailConfirmationStatus()
    {
        DismissAllAlerts();
        var response = await ApiClient.Get<GetMyUserResponse>("user");
        hasConfirmedEmail = response.IsSuccess && response.Result!.IsEmailConfirmed;
        showEmailNotConfirmedAlert = !hasConfirmedEmail;
    }

    async Task Submit()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        NavigationManager.NavigateTo(UrlPath.Checkout2);
    }

    void DismissResendConfirmEmailEmailErrorAlert() => showResendConfirmEmailEmailErrorAlert = false;

    void DismissAllAlerts()
    {
        showEmailNotConfirmedAlert = false;
        showResendConfirmEmailEmailAlert = false;
        showResendConfirmEmailEmailErrorAlert = false;
    }
}
