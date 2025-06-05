using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record SendLockBoxCodesOutput(Seq<Order> UpdatedOrders, Seq<LockBoxCodesEmail> Emails);