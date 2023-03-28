using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class MainLayout
{
    static readonly Dictionary<string, string> urlTitles = new()
    {
        { Urls.OwnerCalendar, "Ejerkalender" },
        { Urls.OwnerOrders, "Ejerbestillinger" },
        { Urls.Orders, "Bestillinger" },
        { Urls.PayIns, "Indbetaling" },
        { Urls.PayOuts, "Udbetaling" },
        { Urls.Postings, "Bogføring" },
        { Urls.Reports, "Rapporter" },
        { Urls.CleaningSchedule, "Rengøringsplan" },
        { Urls.LockBoxCodes, "Nøglebokskoder" },
        { Urls.Users, "Brugere" }
    };

    bool isOpen;
    bool isServerUp = true;
    List<string>? roleUrls;
    ClaimsPrincipal? user;

    [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] public EventAggregator EventAggregator { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        EventAggregator.Subscribe<ServerStatusMessage>(SetServerStatus);
        NavigationManager.LocationChanged += NavigationManagerOnLocationChanged;
        AuthenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        // Notice that the user is retrieved from the access token. The access
        // token expires after a few minutes but is still stored in local
        // storage and will stay there until the user signs in or out. After
        // the access token has expired the client will use the refresh token
        // in the form of a cookie to create a new access token. This will fail
        // if the cookie has expired and at this point the user will be
        // prompted to sign in again..
        //
        // The best way to determine if the user is authenticated is to check
        // for the existence of the access token and then verify that the
        // refresh token cookie still exists. However, the cookie is HTTP only
        // and isn't accessible to the client.
        //
        // Because there's no simple way to determine if an expired access
        // token can be refreshed it's just assumed that it can. In the case
        // where it can't (the cookie has expired) the user will be prompted to
        // log in at the next API call. It's a bit weird so there's room for
        // improvement.
        user = authenticationState.User;
        roleUrls = GetRoleUrs().ToList();
    }

    void NavigationManagerOnLocationChanged(object? sender, LocationChangedEventArgs e) => SetServerStatus(ServerStatusMessage.Up);

    void SetServerStatus(ServerStatusMessage message)
    {
        isServerUp = message.IsUp;
        StateHasChanged();
    }

    async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        var authenticationState = await task;
        user = authenticationState.User;
        roleUrls = GetRoleUrs().ToList();
        StateHasChanged();
    }

    void MenuClick(MouseEventArgs _) => isOpen = !isOpen;

    void MenuItemClick(string url) => NavigationManager.NavigateTo(url);

    IEnumerable<string> GetRoleUrs()
    {
        if (user is null)
            yield break;
        if (user.IsInRole(nameof(Roles.OrderHandling)))
            yield return Urls.OwnerCalendar;
        if (user.IsInRole(nameof(Roles.OrderHandling)) || user.IsInRole(nameof(Roles.Bookkeeping)) || user.IsInRole(nameof(Roles.UserAdministration)))
            yield return Urls.Orders;
        if (user.IsInRole(nameof(Roles.OrderHandling)) || user.IsInRole(nameof(Roles.UserAdministration)))
            yield return Urls.OwnerOrders;
        if (user.IsInRole(nameof(Roles.Bookkeeping)))
        {
            yield return Urls.PayIns;
            yield return Urls.PayOuts;
            yield return Urls.Postings;
        }
        if (user.IsInRole(nameof(Roles.OrderHandling)))
            yield return Urls.Reports;
        if (user.IsInRole(nameof(Roles.OrderHandling)) || user.IsInRole(nameof(Roles.Bookkeeping)) || user.IsInRole(nameof(Roles.UserAdministration)))
            yield return Urls.Users;
        if (user.IsInRole(nameof(Roles.Cleaning)))
            yield return Urls.CleaningSchedule;
        if (user.IsInRole(nameof(Roles.LockBoxCodes)))
            yield return Urls.LockBoxCodes;
    }
}
