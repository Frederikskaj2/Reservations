using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

partial class SignedAmount
{
    [Inject] public CultureInfo CultureInfo { get; set; } = null!;

    [Parameter] public Amount Amount { get; set; }
    [Parameter] public bool AlwaysDisplayDecimals { get; set; }
}
