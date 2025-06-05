using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record SendLockBoxCodesInput(SendLockBoxCodesCommand Command, Seq<Order> Orders, Seq<UserExcerpt> Users, LockBoxCodes LockBoxCodes);