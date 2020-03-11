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

        public Reservation? DraftReservation { get; private set; }

        public PlaceOrderRequest Order { get; } = new PlaceOrderRequest();

        public async Task<IReadOnlyDictionary<int, Resource>> GetResources()
            => (await apiClient.GetJsonAsync<IEnumerable<Resource>>("resources")).ToDictionary(r => r.Id);

        public Task<bool> IsHighPriceDay(LocalDate date)
            => HighPricePolicy.IsHighPriceDay(date, async () => await GetHolidays());

        public async Task<int> GetNumberOfHighPriceDays(Reservation reservation)
        {
            var holidays = await GetHolidays();
            return Enumerable.Range(0, reservation.DurationInDays).Aggregate(
                0,
                (count, i) => count + (HighPricePolicy.IsHighPriceDay(reservation.Date.PlusDays(i), holidays) ? 1 : 0));
        }

        public Task<IEnumerable<ReservedDay>> GetReservedDays(int resourceId, LocalDate fromDate, LocalDate toDate)
            => Task.FromResult(GetAllReservedDays()
                .Where(day => day.ResourceId == resourceId && fromDate <= day.Date && day.Date <= toDate));

        public async Task<IEnumerable<Apartment>> GetApartments()
            => cachedApartments ??= (await apiClient.GetJsonAsync<IEnumerable<Apartment>>("apartments"));

        public void Refresh()
        {
            cachedHolidays = null;
            reservedDaysCacheKey = default;
            cachedReservedDays = null;
        }

        public void CreateDraftReservation(LocalDate date, int durationInDays, Resource resource)
        {
            DraftReservation = new Reservation
            {
                Resource = resource,
                ResourceId = resource.Id,
                Date = date,
                DurationInDays = durationInDays
            };
        }

        public void ClearDraftReservation() => DraftReservation = null;

        public void AddDraftReservationToOrder()
        {
            if (DraftReservation != null)
                Order.Reservations!.Add(DraftReservation);
            ClearDraftReservation();
        }

        public void RemoveReservationFromOrder(Reservation reservation) => Order.Reservations!.Remove(reservation);

        public void ClearOrder() => Order.Reservations!.Clear();

        public async Task<IEnumerable<ReservedDay>> GetReservedDaysAndCacheResult(LocalDate fromDate, LocalDate toDate)
        {
            var cacheKey = (fromDate, toDate);
            if (reservedDaysCacheKey != cacheKey)
            {
                var requestUri = Invariant($"reserved-days?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");
                cachedReservedDays = await apiClient.GetJsonAsync<IEnumerable<ReservedDay>>(requestUri);
                reservedDaysCacheKey = cacheKey;
            }

            return GetAllReservedDays();
        }

        private IEnumerable<ReservedDay> GetAllReservedDays()
        {
            var reservations = cachedReservedDays!.Concat(GetOrderReservedDays());
            if (DraftReservation != null)
                reservations = reservations.Concat(GetReservationReservedDays(DraftReservation));
            return reservations;
        }

        private IEnumerable<ReservedDay> GetOrderReservedDays() => Order.Reservations.SelectMany(GetReservationReservedDays);

        private static IEnumerable<ReservedDay> GetReservationReservedDays(Reservation reservation)
            => Enumerable.Range(0, reservation.DurationInDays).Select(i => new ReservedDay
            {
                Date = reservation.Date.PlusDays(i),
                ResourceId = reservation.ResourceId,
                IsMyReservation = true
            });

        private async Task<HashSet<LocalDate>> GetHolidays()
            => cachedHolidays ??= (await apiClient.GetJsonAsync<IEnumerable<LocalDate>>("holidays")).ToHashSet();
    }
}