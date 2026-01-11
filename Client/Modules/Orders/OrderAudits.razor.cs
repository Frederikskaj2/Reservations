using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

sealed partial class OrderAudits : IDisposable
{
    Timer? timer;

    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public ITimeConverter TimeConverter { get; set; } = null!;

    [Parameter] public OrderDetailsDto? Order { get; set; }

    public void Dispose()
    {
        timer?.Dispose();
        timer = null;
    }

    protected override void OnParametersSet()
    {
        if (Order?.Audits is not null && Order.Audits.Any() && (DateProvider.Now - Order.Audits.Last().Timestamp).TotalMinutes < 60)
            timer = new(_ => OnTimerElapsed(), state: null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    void OnTimerElapsed() => StateHasChanged();

    static string GetAuditName(OrderAuditType type) => type switch
    {
        OrderAuditType.Import => "Importeret fra tidligere system",
        OrderAuditType.PlaceOrder => "Bestilling oprettet",
        OrderAuditType.ConfirmOrder => "Bestilling bekræftet",
        OrderAuditType.SettleReservation => "Reservation opgjort",
        OrderAuditType.CancelReservation => "Reservation afbestilt",
        OrderAuditType.AllowCancellationWithoutFee => "Afbestilling uden gebyr tilladt",
        OrderAuditType.DisallowCancellationWithoutFee => "Afbestilling uden gebyr ej tilladt",
        OrderAuditType.UpdateDescription => "Beskrivelse ændret",
        OrderAuditType.UpdateCleaning => "Tilvalg af rengøring ændret",
        OrderAuditType.FinishOrder => "Bestilling afsluttet",
        OrderAuditType.UpdateReservations => "Reservationer ændret",
        _ => "?",
    };
}
