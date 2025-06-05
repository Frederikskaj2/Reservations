using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

record UpdateResidentReservationsInput(UpdateResidentReservationsCommand Command, Order Order, User User, TransactionId TransactionId);
