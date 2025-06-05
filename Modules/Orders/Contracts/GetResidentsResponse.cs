using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record GetResidentsResponse(IEnumerable<ResidentDto> Residents);
