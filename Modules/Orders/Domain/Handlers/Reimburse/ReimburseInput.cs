using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

record ReimburseInput(ReimburseCommand Command, User User, TransactionId TransactionId);