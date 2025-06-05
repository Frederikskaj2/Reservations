using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

public record Cancellation(OrderId OrderId, Seq<ReservedDay> CancelledReservations);
