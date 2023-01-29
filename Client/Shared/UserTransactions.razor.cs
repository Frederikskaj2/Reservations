using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class UserTransactions
{
    Amount balance;

    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public TransactionId? SelectedTransactionId { get; set; }
    [Parameter] public IEnumerable<UserTransaction>? Transactions { get; set; }

    protected override void OnParametersSet() => balance = Amount.Zero;
}
