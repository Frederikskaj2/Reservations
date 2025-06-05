using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record GetOrdersOutput(Seq<OrderSummary> OrderSummaries);