using System.Threading.Tasks;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    internal class BanquetFacilitiesReservationPolicy : ReservationPolicy, IReservationPolicy
    {
        private const ResourceType ResourceType = Shared.ResourceType.BanquetFacilities;
        private const int HighPriceMinimumNumberOfDays = 1;

        public BanquetFacilitiesReservationPolicy(IDataProvider dataProvider, ReservationsOptions options, IDateProvider dateProvider)
            : base(dataProvider, options, options.Prices[ResourceType], dateProvider)
        {
        }

        public ResourceType Type => ResourceType;

        public override async Task<(int MinimumDays, int MaximumDays)> GetReservationAllowedNumberOfDays(
            int resourceId, LocalDate date)
        {
            var (minimumDays, maximumDays) = await base.GetReservationAllowedNumberOfDays(resourceId, date);
            return !await DataProvider.IsHighPriceDay(date) ? (minimumDays, maximumDays) : (HighPriceMinimumNumberOfDays, maximumDays);
        }

        public override async Task<bool> IsResourceAvailable(LocalDate date, int resourceId)
        {
            var minimumNumberOfDays = await DataProvider.IsHighPriceDay(date) ? HighPriceMinimumNumberOfDays : 1;
            return await IsResourceAvailable(date, minimumNumberOfDays, resourceId);
        }

        public override async Task<bool> IsResourceAvailable(LocalDate date, int durationInDays, int resourceId)
            => await base.IsResourceAvailable(date, durationInDays, resourceId)
               && (durationInDays >= HighPriceMinimumNumberOfDays || !await DataProvider.IsHighPriceDay(date));
    }
}