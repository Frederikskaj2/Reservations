using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class OrderStatus
{
    [Parameter] public IEnumerable<Reservation> Reservations { get; set; } = Enumerable.Empty<Reservation>();
}
