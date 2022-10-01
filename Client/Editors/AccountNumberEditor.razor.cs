using Blazorise;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Editors;

public partial class AccountNumberEditor
{
    TextEdit textEdit = null!;

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public string? Value { get; set; }

    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    public async ValueTask FocusAsync()
    {
        // https://github.com/dotnet/aspnetcore/issues/30070#issuecomment-823938686
        await Task.Yield();
        await textEdit.ElementRef.FocusAsync();
    }
}