using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

record PlaceOwnerOrderInput(PlaceOwnerOrderCommand Command, User User, OrderId OrderId);