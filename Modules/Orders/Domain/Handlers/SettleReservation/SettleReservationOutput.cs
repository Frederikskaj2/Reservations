using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

record SettleReservationOutput(User User, Order Order, Reservation Reservation, Transaction Transaction);