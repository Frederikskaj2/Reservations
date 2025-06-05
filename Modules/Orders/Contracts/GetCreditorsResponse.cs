using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record GetCreditorsResponse(IEnumerable<CreditorDto> Creditors);
