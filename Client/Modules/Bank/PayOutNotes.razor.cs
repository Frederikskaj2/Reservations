using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Core;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

sealed partial class PayOutNotes : IDisposable
{
    static readonly char[] lineSeparators = ['\r', '\n'];

    PayOutNoteDialog payOutNoteDialog = null!;
    Timer? timer;

    [Parameter] public PayOutDetailsDto? PayOut { get; set; }

    [Inject] ApiClient ApiClient { get; set; } = null!;
    [Inject] IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public ITimeConverter TimeConverter { get; set; } = null!;

    [Parameter] public EventCallback<string> OnAddNote { get; set; }

    public void Dispose()
    {
        timer?.Dispose();
        timer = null;
    }

    protected override void OnParametersSet()
    {
        if (PayOut?.Notes is not null && PayOut.Notes.Any() && (DateProvider.Now - PayOut.Notes.Last().Timestamp).TotalMinutes < 60)
            timer = new(_ => OnTimerElapsed(), state: null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    void OnTimerElapsed() => StateHasChanged();

    Task AddNote() => payOutNoteDialog.Show();
}
