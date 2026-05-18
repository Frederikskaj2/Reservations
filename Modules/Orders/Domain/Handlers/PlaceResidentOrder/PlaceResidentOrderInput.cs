using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

record PlaceResidentOrderInput(
    PlaceResidentOrderCommand Command,
    LocalDate Date,
    User Administrator,
    User User,
    OrderId OrderId,
    TransactionId TransactionId);
