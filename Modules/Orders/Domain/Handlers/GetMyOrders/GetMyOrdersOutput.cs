using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record GetMyOrdersOutput(User User, Seq<Order> Orders, HashMap<Reservation, Seq<DatedLockBoxCode>> LockBoxCodes);