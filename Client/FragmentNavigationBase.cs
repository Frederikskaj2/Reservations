using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client;

public class FragmentNavigationBase : ComponentBase, IDisposable
{
    [Inject] IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] NavigationManager NavigationManager { get; set; } = null!;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool isDisposing) => NavigationManager.LocationChanged -= NavigateToFragment;

    protected override void OnInitialized() => NavigationManager.LocationChanged += NavigateToFragment;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await NavigateToFragmentAsync();
    }

    async void NavigateToFragment(object? sender, LocationChangedEventArgs args) => await NavigateToFragmentAsync();

    ValueTask NavigateToFragmentAsync()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        return uri.Fragment.Length is not 0 ? JsRuntime.InvokeVoidAsync("fragmentNavigation.scrollIntoView", uri.Fragment[1..]) : ValueTask.CompletedTask;
    }
}
