using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Calendar;

public sealed partial class Sidebar : IDisposable
{
    IDisposable? draftOrderUpdatedSubscription;
    OrderingOptions? options;
    int reservationCount;
    Price totalPrice = new();

    [Inject] EventAggregator EventAggregator { get; set; } = null!;
    [Inject] ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] IDateProvider DateProvider { get; set; } = null!;
    [Inject] DraftOrder DraftOrder { get; set; } = null!;
    [Inject] Formatter Formatter { get; set; } = null!;

    [Parameter] public EventCallback OnCheckout { get; set; }
    [Parameter] public bool ShowPrices { get; set; }

    public void Dispose() => draftOrderUpdatedSubscription?.Dispose();

    protected override async Task OnInitializedAsync()
    {
        options = await DataProvider.GetOptions();
        if (options is not null)
        {
            draftOrderUpdatedSubscription = EventAggregator.Subscribe<DraftOrderUpdatedMessage>(_ => Update());
            Update();
        }
    }

    void RemoveReservation(DraftReservation reservation) => DraftOrder.RemoveReservation(reservation);

    Task Checkout() => OnCheckout.InvokeAsync();

    void Update()
    {
        reservationCount = DraftOrder.Reservations.Count();
        totalPrice = DraftOrder.Reservations.Sum(GetPrice);
    }

    Price GetPrice(DraftReservation reservation) =>
        Pricing.GetPrice(options!, DateProvider.Holidays, reservation.Extent, reservation.Resource.Type);
}
