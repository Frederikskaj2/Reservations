using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Orders;

public record UpdateResidentOrderRequest(
    string? AccountNumber,
    IReadOnlyCollection<ReservationIndex>? CancelledReservations,
    bool WaiveFee,
    bool AllowCancellationWithoutFee);
