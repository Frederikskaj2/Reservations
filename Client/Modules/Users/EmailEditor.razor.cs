using Blazorise;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class EmailEditor
{
    TextEdit textEdit = null!;

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string Value { get; set; } = "";
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    public ValueTask Focus() => textEdit.ElementRef.FocusAsync();
}
