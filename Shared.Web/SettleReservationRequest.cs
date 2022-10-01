using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Shared.Web;

public class SettleReservationRequest
{
    public UserId UserId { get; set; }
    public ReservationId ReservationId { get; set; }
    public Amount Damages { get; set; }
    public string? Description { get; set; }
}
