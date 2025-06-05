using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class PhoneEditor
{
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string Value { get; set; } = "";
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
}
