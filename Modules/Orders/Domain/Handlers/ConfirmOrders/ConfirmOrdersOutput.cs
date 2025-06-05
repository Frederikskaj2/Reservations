using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record ConfirmOrdersOutput(Seq<UserWithOrders> UsersWithOrders);