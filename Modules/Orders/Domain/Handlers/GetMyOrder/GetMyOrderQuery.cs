using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record GetMyOrderQuery(LocalDate Today, UserId UserId, OrderId OrderId);
