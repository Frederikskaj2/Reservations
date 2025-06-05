using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

public record UserWithOrders(User User, Seq<Order> Orders);
