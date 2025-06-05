using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

record PlaceMyOrderInput(
    PlaceMyOrderCommand Command,
    LocalDate Date,
    User User,
    LockBoxCodes LockBoxCodes,
    OrderId OrderId,
    TransactionId TransactionId);