using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

record GetOrdersInput(OrderingOptions Options, LocalDate Today, Seq<OrderExcerpt> Orders, Seq<UserExcerpt> Users);
