using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record GetMyOrderOutput(Order Order, User User, HashMap<Reservation, Seq<DatedLockBoxCode>> LockBoxCodes);