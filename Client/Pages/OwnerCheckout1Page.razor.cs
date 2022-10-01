using Blazorise;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.OrderHandling))]
public partial class OwnerCheckout1Page
{
    string? description;
    bool isCleaningRequired = true;
    OrderingOptions? options;
    bool showErrorAlert;
    bool showReservationConflictAlert;
    Validations validations = null!;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public DraftOrder DraftOrder { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync() => options = await DataProvider.GetOptionsAsync();

    async Task SubmitAsync()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;

        var request = new PlaceOwnerOrderRequest
        {
            Description = description,
            Reservations = DraftOrder.GetReservationRequests().ToArray(),
            IsCleaningRequired = isCleaningRequired
        };
        var response = await ApiClient.PostAsync<OwnerOrder>("orders/owner", request);
        if (response.IsSuccess)
        {
            DraftOrder.Clear();
            NavigationManager.NavigateTo(Urls.OwnerCheckout2);
        }
        else if (response.Problem!.Status == HttpStatusCode.Conflict)
            showReservationConflictAlert = true;
        else
            showErrorAlert = true;
    }

    void DismissReservationConflictAlert() => showReservationConflictAlert = false;

    void DismissErrorAlert() => showErrorAlert = false;
}
