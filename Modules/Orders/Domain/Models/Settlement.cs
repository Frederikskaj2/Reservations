using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

public record Settlement(OrderId OrderId, ReservedDay Reservation, Option<string> Damages);
