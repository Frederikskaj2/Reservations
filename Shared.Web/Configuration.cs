using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Web;

public record Configuration(
    OrderingOptions Options,
    IEnumerable<Resource> Resources,
    IEnumerable<Apartment> Apartments,
    IEnumerable<AccountName> AccountNames);
