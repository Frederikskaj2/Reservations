using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record GetOrdersQuery(LocalDate Today, bool IncludeResidentOrders, bool IncludeOwnerOrders, Seq<OrderId> OrderIds);
