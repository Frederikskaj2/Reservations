namespace Frederikskaj2.Reservations.Client
{
    public class ReservationsOptions
    {
        public int ReservationIsNotAllowedBeforeDaysFromNow { get; set; } = 5;
        public int ReservationIsNotAllowedAfterDaysFromNow { get; set; } = 270;
    }
}