using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

record SettleReservationInput(SettleReservationCommand Command, User User, Order Order, TransactionId TransactionId);