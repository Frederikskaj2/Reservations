using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Persistence.UnitTests;

class ResidentOrderClass
{
    public OrderId OrderId { get; init; }
    public Resident? Order { get; init; }
}