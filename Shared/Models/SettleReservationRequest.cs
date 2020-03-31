using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Shared
{
    public class SettleReservationRequest
    {
        public int ReservationId { get; set; }

        public int Damages { get; set; }
    }
}