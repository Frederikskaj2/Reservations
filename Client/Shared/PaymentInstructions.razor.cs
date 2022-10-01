using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class PaymentInstructions
{
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public PaymentInformation? PaymentInformation { get; set; }
    [Parameter] public UserId UserId { get; set; }
    [Parameter] public bool ShowLinkToTransactions { get; set; }
}
