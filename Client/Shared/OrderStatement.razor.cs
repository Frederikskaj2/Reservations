using Blazorise;
using Frederikskaj2.Reservations.Client.Dialogs;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Shared;

public sealed partial class OrderStatement : IDisposable
{
    static readonly TimeSpan timerInterval = TimeSpan.FromMinutes(1);
    readonly HashSet<ReservationIndex> cancelledReservations = new();
    string? accountNumber;
    bool allowCancellationWithoutFee;
    Instant now;
    OrderingOptions? options;
    Price? price;
    IReadOnlyDictionary<ResourceId, Resource>? resources;
    Timer? timer;
    Validations validations = null!;
    bool waiveFee;

    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public bool IsHistoryOrder { get; set; }
    [Parameter] public string? AccountNumber { get; set; }
    [Parameter] public IEnumerable<Reservation> Reservations { get; set; } = Enumerable.Empty<Reservation>();
    [Parameter] public Instant? NoFeeCancellationIsAllowedBefore { get; set; }
    [Parameter] public bool CanEditOrder { get; set; }
    [Parameter] public bool CanWaiveFee { get; set; }
    [Parameter] public EventCallback<ReservationIndex> OnSettleReservation { get; set; }

    [Parameter]
    public EventCallback<(string AccountNumber, HashSet<ReservationIndex> CancelledReservations, bool WaiveFee, bool AllowCancellationWithoutFee)> OnSubmit
    {
        get;
        set;
    }

    bool IsUpdated =>
        AccountNumber != accountNumber?.Trim() || cancelledReservations.Count > 0 || allowCancellationWithoutFee != now <= NoFeeCancellationIsAllowedBefore;

    public void Dispose() => timer?.Dispose();

    protected override async Task OnInitializedAsync()
    {
        options = await DataProvider.GetOptionsAsync();
        resources = await DataProvider.GetResourcesAsync();

        UpdatePrice();

        now = DateProvider.Now;
        if (now <= NoFeeCancellationIsAllowedBefore)
        {
            waiveFee = true;
            timer = new Timer(OnTimerElapsed);
            timer.Change(timerInterval, timerInterval);
        }
    }

    protected override void OnParametersSet()
    {
        now = DateProvider.Now;
        accountNumber = AccountNumber;
        cancelledReservations.Clear();
        allowCancellationWithoutFee = now <= NoFeeCancellationIsAllowedBefore;
    }

    void ToggleCancelReservation(ReservationIndex reservationIndex)
    {
        if (!cancelledReservations.Contains(reservationIndex))
            cancelledReservations.Add(reservationIndex);
        else
            cancelledReservations.Remove(reservationIndex);
        UpdatePrice();
    }

    Task SettleReservationAsync(ReservationIndex reservation) => OnSettleReservation.InvokeAsync(reservation);

    async Task SubmitAsync()
    {
        await validations.ClearAll();
        if (!await validations.ValidateAll())
            return;
        await validations.ClearAll();
        // Clone the collection because OnParametersSet may be invoked while OnSubmit awaits.
        await OnSubmit.InvokeAsync((accountNumber!, cancelledReservations.ToHashSet(), waiveFee, allowCancellationWithoutFee));
    }

    void UpdatePrice() =>
        price = Reservations
            .Where((reservation, index) => reservation.Status is not (ReservationStatus.Abandoned or ReservationStatus.Cancelled) &&
                                           !cancelledReservations.Contains(index))
            .Sum(reservation => reservation.Price!);

    void OnTimerElapsed(object? _)
    {
        now = DateProvider.Now;
        waiveFee = now <= NoFeeCancellationIsAllowedBefore;
        StateHasChanged();
        if (waiveFee)
            return;
        timer!.Dispose();
        timer = null;
    }
}
