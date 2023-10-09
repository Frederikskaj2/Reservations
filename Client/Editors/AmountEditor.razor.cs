using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Components;
using System;

namespace Frederikskaj2.Reservations.Client.Editors;

public partial class AmountEditor
{
    [Parameter] public bool AutoFocus { get; set; }
    [Parameter] public int MaximumValue { get; set; } = ValidationRules.MaximumAmount;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public int Value { get; set; }
    [Parameter] public EventCallback<int> ValueChanged { get; set; }
}
