using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

public record SettleReservationRequest(ReservationIndex ReservationIndex, Amount Damages, string? Description);
