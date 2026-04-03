using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Core;

public class FragmentNavigationBase : ComponentBase, IDisposable
{
    [Inject] IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] NavigationManager NavigationManager { get; set; } = null!;

    public void Dispose()
    {
        Dispose(isDisposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool isDisposing) => NavigationManager.LocationChanged -= NavigateToFragment;

    protected override void OnInitialized() => NavigationManager.LocationChanged += NavigateToFragment;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await NavigateToFragmentAsync();
    }

    void NavigateToFragment(object? sender, LocationChangedEventArgs args) => _ = NavigateToFragmentAsync();

    async Task NavigateToFragmentAsync()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        if (uri.Fragment.Length is not 0)
            await JsRuntime.InvokeVoidAsync("fragmentNavigation.scrollIntoView", uri.Fragment[1..]);
    }
}
