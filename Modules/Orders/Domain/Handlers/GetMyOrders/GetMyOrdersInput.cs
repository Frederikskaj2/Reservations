using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record GetMyOrdersInput(GetMyOrdersQuery Query, User User, Seq<Order> Orders, LockBoxCodes LockBoxCodes);