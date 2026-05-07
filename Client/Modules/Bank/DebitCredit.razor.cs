using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

public partial class DebitCredit
{
    [Inject] Formatter Formatter { get; set; } = null!;

    [Parameter] public Amount Amount { get; set; }
}
