using Frederikskaj2.Reservations.Orders;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

partial class PaymentInstructions
{
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public PaymentInformation? PaymentInformation { get; set; }
    [Parameter] public bool ShowLinkToTransactions { get; set; }
}
