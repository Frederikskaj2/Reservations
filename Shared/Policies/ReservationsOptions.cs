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
                    HighRentPerDay = 1000,
                    CleaningFee = 500,
                    LowDeposit = 1000,
                    HighDeposit = 3000
                }
            },
            {
                ResourceType.Bedroom, new PriceOptions
                {
                    LowRentPerDay = 200,
                    HighRentPerDay = 250,
                    CleaningFee = 200,
                    LowDeposit = 500,
                    HighDeposit = 500
                }
            }
        };

        public int ReservationIsNotAllowedBeforeDaysFromNow { get; set; } = 5;
        public int ReservationIsNotAllowedAfterDaysFromNow { get; set; } = 270;
        public int MaximumAllowedReservationDays { get; set; } = 7;
        public LocalTime CheckInTime { get; set; } = new LocalTime(12, 0);
        public LocalTime CheckOutTime { get; set; } = new LocalTime(10, 0);
        public int CancellationFee { get; set; } = 200;
        public int MinimumCancellationNoticeInDays { get; set; } = 14;
        public int HighlightUnpaidOrdersAfterDays { get; set; } = 7;
        public string PayInAccountNumber { get; set; } = "9444-12501110";
    }
}