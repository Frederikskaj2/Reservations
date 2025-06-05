using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record GetOrdersResponse(IEnumerable<OrderSummaryDto> Orders);
