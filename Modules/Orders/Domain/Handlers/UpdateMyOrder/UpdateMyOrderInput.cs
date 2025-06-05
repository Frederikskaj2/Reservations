using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record UpdateMyOrderInput(UpdateMyOrderCommand Command, User User, Order Order, Option<TransactionId> TransactionIdOption);
