using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

public record ReservationsUpdate(OrderId OrderId, Seq<ReservedDay> UpdatedReservations);
