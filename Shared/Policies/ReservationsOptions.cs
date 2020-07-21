using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class ReservationsOptions
    {
        public IReadOnlyDictionary<ResourceType, PriceOptions> Prices { get; } = new Dictionary<ResourceType, PriceOptions>
        {
            {
                ResourceType.BanquetFacilities, new PriceOptions
                {
                    LowRentPerDay = 500,
                    HighRentPerDay = 750,
                    Cleaning = 500,
                    LowDeposit = 1000,
                    HighDeposit = 3000
                }
            },
            {
                ResourceType.Bedroom, new PriceOptions
                {
                    LowRentPerDay = 200,
                    HighRentPerDay = 250,
                    Cleaning = 200,
                    LowDeposit = 500,
                    HighDeposit = 500
                }
            }
        };

        public int ReservationIsNotAllowedBeforeDaysFromNow { get; set; } = 5;
        public int ReservationIsNotAllowedAfterDaysFromNow { get; set; } = 270;
        // The key code logic expects that at most two key codes are required for a single reservation.
        public int MaximumAllowedReservationDays { get; set; } = 7;
        public LocalTime CheckInTime { get; set; } = new LocalTime(12, 0);
        public LocalTime CheckOutTime { get; set; } = new LocalTime(10, 0);
        public int CancellationFee { get; set; } = 200;
        public int MinimumCancellationNoticeInDays { get; set; } = 14;
        public int PaymentDeadlineInDays { get; set; } = 7;
        public string PayInAccountNumber { get; set; } = "9444-12501110";
        public int RevealLockBoxCodeDaysBeforeReservationStart { get; set; } = 3;
    }
}