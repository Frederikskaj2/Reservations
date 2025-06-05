using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Core;

public sealed partial class Frederikskaj2Carousel : IDisposable
{
    static readonly TimeSpan slideshowInterval = TimeSpan.FromSeconds(5);
    readonly Timer timer;
    int currentIndex = 1;
    bool isSlideshowEnabled = true;

    public Frederikskaj2Carousel() => timer = new(OnTimerElapsed);

    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter] public string? BaseName { get; set; }
    [Parameter] public int Count { get; set; }
    [Parameter] public string? Description { get; set; }

    public void Dispose() => timer.Dispose();

    protected override bool ShouldRender() => false;

    protected override void OnInitialized() => ResetTimer();

    async Task Navigate(int delta)
    {
        if (isSlideshowEnabled)
            ResetTimer();
        currentIndex = (Count + currentIndex + delta - 1)%Count + 1;
        await JsRuntime.InvokeVoidAsync("carouselSelectImage", currentIndex - 1);
    }

    async Task NavigateKeyUp(KeyboardEventArgs e, int delta)
    {
        if (e.Key == "Enter")
            await Navigate(delta);
    }

    void ToggleSlideshow()
    {
        isSlideshowEnabled = !isSlideshowEnabled;
        if (isSlideshowEnabled)
            ResetTimer();
        else
            DisableTimer();
    }

    void ToggleSlideshowKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            ToggleSlideshow();
    }

    void ResetTimer() => timer.Change(slideshowInterval, slideshowInterval);

    void DisableTimer() => timer.Change(Timeout.Infinite, Timeout.Infinite);

    void OnTimerElapsed(object? state) => _ = Navigate(1);
}
