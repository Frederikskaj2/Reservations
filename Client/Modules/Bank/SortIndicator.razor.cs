using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

partial class SortIndicator
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public bool IsDescending { get; set; }
}
