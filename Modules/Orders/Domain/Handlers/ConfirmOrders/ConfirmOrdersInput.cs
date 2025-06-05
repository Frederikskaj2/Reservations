using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record ConfirmOrdersInput(ConfirmOrdersCommand Command, Seq<User> Users, Seq<Order> UnconfirmedOrders, Seq<TransactionExcerpt> Transactions);