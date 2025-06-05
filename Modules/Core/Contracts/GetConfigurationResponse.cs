using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Core;

public record GetConfigurationResponse(
    OrderingOptions Options,
    IEnumerable<Resource> Resources,
    IEnumerable<Apartment> Apartments,
    IEnumerable<AccountName> AccountNames);
