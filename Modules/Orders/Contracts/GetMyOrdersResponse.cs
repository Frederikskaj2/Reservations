using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record GetMyOrdersResponse(IEnumerable<MyOrderDto> Orders);
