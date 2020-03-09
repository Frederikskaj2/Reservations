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
        private IEnumerable<Reservation>? cachedReservations;
        private (LocalDate FromDate, LocalDate ToDate) reservationsCacheKey;

        public ClientDataProvider(ApiClient apiClient)
            => this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

        public Reservation? DraftReservation { get; private set; }

        public Order Order { get; } = new Order { Reservations = new List<Reservation>() };

        public Task<bool> IsHighPriceDay(LocalDate date)
            => HighPricePolicy.IsHighPriceDay(date, async () => await GetHolidays());

        public async Task<int> GetNumberOfHighPriceDays(Reservation reservation)
        {
            var holidays = await GetHolidays();
            return Enumerable.Range(0, reservation.DurationInDays).Aggregate(
                0,
                (count, i) => count + (HighPricePolicy.IsHighPriceDay(reservation.Date.PlusDays(i), holidays) ? 1 : 0));
        }

        public Task<IEnumerable<Reservation>> GetReservations(int resourceId, LocalDate fromDate, LocalDate toDate)
            => Task.FromResult(
                GetAllReservations().Where(
                    rr => rr.ResourceId == resourceId && fromDate <= rr.Date && rr.Date <= toDate));

        public async Task<IEnumerable<Apartment>> GetApartments()
            => cachedApartments ??= (await apiClient.GetJsonAsync<IEnumerable<Apartment>>("apartments"));

        public void Refresh()
        {
            cachedHolidays = null;
            cachedReservations = null;
        }

        public void CreateDraftReservation(LocalDate date, int durationInDays, Resource resource)
        {
            DraftReservation = new Reservation
            {
                Resource = resource,
                ResourceId = resource.Id,
                Date = date,
                DurationInDays = durationInDays,
                IsMyReservation = true
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

        public async Task<IReadOnlyDictionary<int, Resource>> GetResources()
            => (await apiClient.GetJsonAsync<IEnumerable<Resource>>("resources")).ToDictionary(r => r.Id);

        public async Task<IEnumerable<Reservation>> GetReservationsAndCacheResult(LocalDate fromDate, LocalDate toDate)
        {
            var cacheKey = (fromDate, toDate);
            if (reservationsCacheKey != cacheKey)
            {
                var requestUri = Invariant($"reservations?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");
                cachedReservations = await apiClient.GetJsonAsync<IEnumerable<Reservation>>(requestUri);
                reservationsCacheKey = cacheKey;
            }

            return GetAllReservations();
        }

        private IEnumerable<Reservation> GetAllReservations()
        {
            var reservations = cachedReservations!.Concat(Order.Reservations);
            if (DraftReservation != null)
                reservations = reservations.Concat(new[] { DraftReservation });
            return reservations;
        }

        private async Task<HashSet<LocalDate>> GetHolidays()
            => cachedHolidays ??= (await apiClient.GetJsonAsync<IEnumerable<LocalDate>>("holidays")).ToHashSet();
    }
}