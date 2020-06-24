using System;
using System.Threading.Tasks;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    internal class BanquetFacilitiesReservationPolicy : ReservationPolicy, IReservationPolicy
    {
        private const ResourceType ResourceType = Shared.ResourceType.BanquetFacilities;
        private const int HighPriceMinimumNumberOfDays = 2;

        public BanquetFacilitiesReservationPolicy(IDataProvider dataProvider, ReservationsOptions options, IDateProvider dateProvider)
            : base(dataProvider, options, options.Prices[ResourceType], dateProvider)
        {
        }

        public ResourceType Type => ResourceType;

        public override async Task<(int MinimumDays, int MaximumDays)> GetReservationAllowedNumberOfDays(
            int resourceId, LocalDate date, bool includeOrder)
        {
            var (minimumDays, maximumDays) = await base.GetReservationAllowedNumberOfDays(resourceId, date, includeOrder);
            return !DataProvider.IsHighPriceDay(date) ? (minimumDays, maximumDays) : (Math.Min(HighPriceMinimumNumberOfDays, maximumDays), maximumDays);
        }

        public override async Task<bool> IsResourceAvailable(LocalDate date, int resourceId, bool includeOrder)
        {
            var minimumNumberOfDays = DataProvider.IsHighPriceDay(date) ? HighPriceMinimumNumberOfDays : 1;
            return await IsResourceAvailable(date, minimumNumberOfDays, resourceId, includeOrder);
        }

        protected override async Task<bool> IsResourceAvailable(
            LocalDate date, int durationInDays, int resourceId, bool includeOrder)
            => await base.IsResourceAvailable(date, durationInDays, resourceId, includeOrder)
               && (durationInDays >= HighPriceMinimumNumberOfDays || !DataProvider.IsHighPriceDay(date));
    }
}