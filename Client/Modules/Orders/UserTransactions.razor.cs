using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

partial class UserTransactions
{
    Amount balance;

    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public TransactionId? SelectedTransactionId { get; set; }
    [Parameter] public IEnumerable<TransactionDto>? Transactions { get; set; }

    protected override void OnParametersSet() => balance = Amount.Zero;
}
