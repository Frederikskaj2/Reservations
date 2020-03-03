using System;

namespace Frederikskaj2.Reservations.Client
{
    public class ReservationsOptions
    {
        public int ReservationIsNotAllowedBeforeDaysFromNow { get; set; } = 5;
        public int ReservationIsNotAllowedAfterDaysFromNow { get; set; } = 270;
        public TimeSpan CheckInTime { get; set; } = TimeSpan.FromHours(12);
        public TimeSpan CheckOutTime { get; set; } = TimeSpan.FromHours(10);
    }
}