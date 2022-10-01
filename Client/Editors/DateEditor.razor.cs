using Blazorise;
using Microsoft.AspNetCore.Components;
using NodaTime;
using System;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Editors;

public partial class DateEditor
{
    DateEdit<DateTime> dateEdit = null!;
    DateTime dateTime;

    [Parameter] public LocalDate Date { get; set; }
    [Parameter] public EventCallback<LocalDate> DateChanged { get; set; }

    protected override void OnParametersSet() => dateTime = Date.AtMidnight().ToDateTimeUnspecified();

    async Task DateChangedAsync(DateTime value)
    {
        dateTime = value;
        Date = LocalDate.FromDateTime(dateTime);
        await DateChanged.InvokeAsync(Date);
    }

    public async ValueTask FocusAsync()
    {
        // https://github.com/dotnet/aspnetcore/issues/30070#issuecomment-823938686
        await Task.Yield();
        await dateEdit.ElementRef.FocusAsync();
    }
}