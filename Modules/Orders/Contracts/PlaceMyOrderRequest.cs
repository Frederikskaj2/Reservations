using Frederikskaj2.Reservations.Users;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record PlaceMyOrderRequest(
    string? FullName,
    string? Phone,
    ApartmentId? ApartmentId,
    IEnumerable<ReservationRequest>? Reservations,
    string? AccountNumber);
