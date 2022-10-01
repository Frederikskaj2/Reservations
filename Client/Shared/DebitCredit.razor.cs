using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class DebitCredit
{
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Parameter] public Amount Amount { get; set; }
}
