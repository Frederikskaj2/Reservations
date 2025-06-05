using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record FinishOwnerOrdersInput(FinishOwnerOrdersCommand Command, Seq<Order> Orders);