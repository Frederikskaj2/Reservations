using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

public partial class DebitCredit
{
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public Amount Amount { get; set; }
}
