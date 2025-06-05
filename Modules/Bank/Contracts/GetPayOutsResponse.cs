using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Bank;

public record GetPayOutsResponse(IEnumerable<PayOutDto> PayOuts);
