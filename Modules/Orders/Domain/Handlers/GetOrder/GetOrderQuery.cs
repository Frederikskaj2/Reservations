using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record GetOrderQuery(LocalDate Today, OrderId OrderId);
