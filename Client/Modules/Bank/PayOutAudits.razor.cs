using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Core;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

sealed partial class PayOutAudits : IDisposable
{
    Timer? timer;

    [Inject] IDateProvider DateProvider { get; set; } = null!;
    [Inject] Formatter Formatter { get; set; } = null!;
    [Inject] ITimeConverter TimeConverter { get; set; } = null!;

    [Parameter] public PayOutDetailsDto? PayOut { get; set; }

    public void Dispose()
    {
        timer?.Dispose();
        timer = null;
    }

    protected override void OnParametersSet()
    {
        if (PayOut?.Audits is not null && PayOut.Audits.Any() && (DateProvider.Now - PayOut.Audits.Last().Timestamp).TotalMinutes < 60)
            timer = new(_ => OnTimerElapsed(), state: null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    void OnTimerElapsed() => StateHasChanged();

    static string GetAuditName(PayOutAuditType type) =>
        type switch
        {
            PayOutAuditType.Create => "Udbetaling oprettet",
            PayOutAuditType.Cancel => "Udbetaling annulleret",
            PayOutAuditType.UpdateAccountNumber => "Kontonummer ændret",
            PayOutAuditType.AddNote => "Notat tilføjet",
            PayOutAuditType.Reconcile => "Udbetaling afstemt",
            _ => "?",
        };
}
