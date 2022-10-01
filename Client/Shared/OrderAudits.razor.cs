using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Shared;

public sealed partial class OrderAudits : IDisposable
{
    Timer? timer;

    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public OrderDetails? Order { get; set; }

    public void Dispose()
    {
        timer?.Dispose();
        timer = null;
    }

    protected override void OnParametersSet()
    {
        if (Order?.Audits is not null && Order.Audits.Any() && (DateProvider.Now - Order.Audits.First().Timestamp).TotalMinutes < 60)
            timer = new Timer(_ => OnTimerElapsed(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    void OnTimerElapsed() => StateHasChanged();

    static string GetAuditName(OrderAuditType type) => type switch
    {
        OrderAuditType.Import => "Import fra tidligere system",
        OrderAuditType.PlaceOrder => "Oprettelse af bestilling",
        OrderAuditType.ConfirmOrder => "Bekræftelse af bestilling",
        OrderAuditType.SettleReservation => "Opgørelse af reservation",
        OrderAuditType.CancelReservation => "Afbestilling af reservation",
        OrderAuditType.AllowCancellationWithoutFee => "Tillad afbestilling uden gebyr",
        OrderAuditType.DisallowCancellationWithoutFee => "Forbyd afbestilling uden gebyr",
        OrderAuditType.UpdateDescription => "Ændring af beskrivelse",
        OrderAuditType.UpdateCleaning => "Ændring af rengøring",
        OrderAuditType.FinishOrder => "Afslutning af bestilling",
        _ => "?"
    };
}
