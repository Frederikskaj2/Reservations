using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class ReservationsOptions
    {
        public int ReservationIsNotAllowedBeforeDaysFromNow { get; set; } = 5;
        public int ReservationIsNotAllowedAfterDaysFromNow { get; set; } = 270;
        public LocalTime CheckInTime { get; set; } = new LocalTime(12, 0);
        public LocalTime CheckOutTime { get; set; } = new LocalTime(10, 0);
    }
}