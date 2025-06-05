using Frederikskaj2.Reservations.Users;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record PlaceResidentOrderRequest(
    UserId UserId,
    string? FullName,
    string? Phone,
    ApartmentId? ApartmentId,
    string? AccountNumber,
    IEnumerable<ReservationRequest>? Reservations);
