using Frederikskaj2.Reservations.Bank;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

partial class PayOutStatus
{
    [Parameter] public PayOutDetailsDto? PayOut { get; set; }
}
