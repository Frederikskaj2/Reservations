using Frederikskaj2.Reservations.Orders;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Client.Modules.Orders;

partial class OrderStatus
{
    [Parameter] public IEnumerable<ReservationDto> Reservations { get; set; } = [];
}
