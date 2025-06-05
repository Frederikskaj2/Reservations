using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

record GetMyOrderInput(GetMyOrderQuery Query, Order Order, User User, LockBoxCodes LockBoxCodes);
