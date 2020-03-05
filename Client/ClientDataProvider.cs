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
        private HashSet<LocalDate>? cachedHolidays;
        private IEnumerable<ResourceReservation>? cachedReservations;
        private (LocalDate FromDate, LocalDate ToDate) reservationsCacheKey;

        public ClientDataProvider(ApiClient apiClient)
            => this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

        public Task<bool> IsHighPriceDay(LocalDate date)
            => HighPricePolicy.IsHighPriceDay(date, async () => await GetHolidays());

        public async Task<int> GetNumberOfHighPriceDays(ResourceReservation reservation)
        {
            var holidays = await GetHolidays();
            return Enumerable.Range(0, reservation.DurationInDays).Aggregate(
                0,
                (count, i) => count + (HighPricePolicy.IsHighPriceDay(reservation.Date.PlusDays(i), holidays) ? 1 : 0));
        }

        public Task<IEnumerable<ResourceReservation>> GetReservations(
            int resourceId, LocalDate fromDate, LocalDate toDate)
            => Task.FromResult(
                cachedReservations.Where(
                    rr => rr.ResourceId == resourceId && fromDate <= rr.Date && rr.Date <= toDate));

        public void Refresh()
        {
            cachedHolidays = null;
        }

        public async Task<IReadOnlyDictionary<int, Resource>> GetResources()
            => (await apiClient.GetJsonAsync<IEnumerable<Resource>>("resources")).ToDictionary(r => r.Id);


        public async Task<IEnumerable<ResourceReservation>> GetReservationsAndCacheResult(
            LocalDate fromDate, LocalDate toDate)
        {
            var cacheKey = (fromDate, toDate);
            if (reservationsCacheKey != cacheKey)
            {
                var requestUri = Invariant(
                    $"resource-reservations?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");
                cachedReservations = await apiClient.GetJsonAsync<IEnumerable<ResourceReservation>>(requestUri);
                reservationsCacheKey = cacheKey;
            }

            return cachedReservations!;
        }

        private async Task<HashSet<LocalDate>> GetHolidays()
            => cachedHolidays ??= (await apiClient.GetJsonAsync<IEnumerable<LocalDate>>("holidays")).ToHashSet();
    }
}