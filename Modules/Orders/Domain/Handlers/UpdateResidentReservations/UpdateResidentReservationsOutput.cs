using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

record UpdateResidentReservationsOutput(Order Order, User User, Transaction Transaction);