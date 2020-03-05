using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class ReservationsOptions
    {
        public Dictionary<ResourceType, PriceOptions> Prices = new Dictionary<ResourceType, PriceOptions>
        {
            {
                ResourceType.BanquetFacilities, new PriceOptions
                {
                    LowRentPerDay = 500M,
                    HighRentPerDay = 1000M,
                    CleaningFee = 500M,
                    LowDeposit = 1000M,
                    HighDeposit = 3000M
                }
            },
            {
                ResourceType.Bedroom, new PriceOptions
                {
                    LowRentPerDay = 200M,
                    HighRentPerDay = 250M,
                    CleaningFee = 200M,
                    LowDeposit = 500M,
                    HighDeposit = 500M
                }
            }
        };

        public int ReservationIsNotAllowedBeforeDaysFromNow { get; set; } = 5;
        public int ReservationIsNotAllowedAfterDaysFromNow { get; set; } = 270;
        public int MaximumAllowedReservationDays { get; set; } = 7;
        public LocalTime CheckInTime { get; set; } = new LocalTime(12, 0);
        public LocalTime CheckOutTime { get; set; } = new LocalTime(10, 0);
    }
}