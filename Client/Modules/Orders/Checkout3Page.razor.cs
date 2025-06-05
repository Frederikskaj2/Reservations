using Frederikskaj2.Reservations.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

[Authorize(Roles = nameof(Roles.Resident))]
partial class Checkout3Page
{
    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
}
