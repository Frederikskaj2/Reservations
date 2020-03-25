using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using NodaTime;
using static System.FormattableString;

namespace Frederikskaj2.Reservations.Client
{
    public class ClientDataProvider : IDataProvider
    {
        private readonly ApiClient apiClient;
        private IEnumerable<Apartment>? cachedApartments;
        private HashSet<LocalDate>? cachedHolidays;
        private IEnumerable<ReservedDay>? cachedReservedDays;
        private (LocalDate FromDate, LocalDate ToDate) reservedDaysCacheKey;

        public ClientDataProvider(ApiClient apiClient)
            => this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

        public DraftOrder DraftOrder { get; private set; } = new DraftOrder();

        public async Task<IReadOnlyDictionary<int, Resource>> GetResources()
        {
            var maybe = await apiClient.GetJsonAsync<IEnumerable<Resource>>("resources");
            if (maybe.TryGetValue(out var resources))
                return resources.ToDictionary(resource => resource.Id);
            return new Dictionary<int, Resource>();
        }

        public Task<bool> IsHighPriceDay(LocalDate date)
            => HighPricePolicy.IsHighPriceDay(date, async () => await GetHolidays());

        public async Task<int> GetNumberOfHighPriceDays(LocalDate date, int durationInDays)
        {
            var holidays = await GetHolidays();
            return Enumerable.Range(0, durationInDays).Aggregate(
                0,
                (count, i) => count + (HighPricePolicy.IsHighPriceDay(date.PlusDays(i), holidays) ? 1 : 0));
        }

        public Task<IEnumerable<ReservedDay>> GetReservedDays(int resourceId, LocalDate fromDate, LocalDate toDate)
            => Task.FromResult(
                GetAllReservedDays()
                    .Where(day => day.ResourceId == resourceId && fromDate <= day.Date && day.Date <= toDate));

        public void Refresh()
        {
            cachedHolidays = null;
            reservedDaysCacheKey = default;
            cachedReservedDays = null;
        }

        public async Task<IEnumerable<Apartment>> GetApartments()
        {
            if (cachedApartments is null)
            {
                var maybe = await apiClient.GetJsonAsync<IEnumerable<Apartment>>("apartments");
                if (!maybe.TryGetValue(out cachedApartments))
                    cachedApartments = Enumerable.Empty<Apartment>();
            }
            return cachedApartments;
        }

        public async Task<IEnumerable<ReservedDay>> GetReservedDaysAndCacheResult(LocalDate fromDate, LocalDate toDate)
        {
            var cacheKey = (fromDate, toDate);
            if (reservedDaysCacheKey != cacheKey)
            {
                var requestUri = Invariant($"reserved-days?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");
                var maybe = await apiClient.GetJsonAsync<IEnumerable<ReservedDay>>(requestUri);
                if (maybe.TryGetValue(out cachedReservedDays))
                {
                    reservedDaysCacheKey = cacheKey;
                }
                else
                {
                    cachedReservedDays = Enumerable.Empty<ReservedDay>();
                    reservedDaysCacheKey = default;
                }
            }

            return GetAllReservedDays();
        }

        private IEnumerable<ReservedDay> GetAllReservedDays()
        {
            var reservations = cachedReservedDays!.Concat(GetOrderReservedDays());
            if (DraftOrder.DraftReservation != null)
                reservations = reservations.Concat(GetReservationReservedDays(DraftOrder.DraftReservation));
            return reservations;
        }

        private IEnumerable<ReservedDay> GetOrderReservedDays()
            => DraftOrder.Reservations.SelectMany(GetReservationReservedDays);

        private static IEnumerable<ReservedDay> GetReservationReservedDays(DraftReservation reservation)
            => Enumerable.Range(0, reservation.DurationInDays).Select(
                i => new ReservedDay { Date = reservation.Date.PlusDays(i), ResourceId = reservation.Resource.Id, IsMyReservation = true });

        private async Task<HashSet<LocalDate>> GetHolidays()
        {
            if (cachedHolidays == null)
            {
                var maybe = await apiClient.GetJsonAsync<IEnumerable<LocalDate>>("holidays");
                cachedHolidays = maybe.TryGetValue(out var holidays) ? holidays.ToHashSet() : new HashSet<LocalDate>();
            }
            return cachedHolidays;
        }
    }
}