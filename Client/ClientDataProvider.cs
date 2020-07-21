using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using NodaTime;

namespace Frederikskaj2.Reservations.Client
{
    public class ClientDataProvider : IDataProvider
    {
        private readonly ApiClient apiClient;
        private IEnumerable<Apartment>? cachedApartments;
        private IEnumerable<ReservedDay>? cachedReservedDays;
        private IReadOnlyDictionary<int, Resource>? cachedResources;
        private IEnumerable<WeeklyLockBoxCodes>? cachedWeeklyLockBoxCodes;
        private readonly Lazy<HashSet<LocalDate>> holidays;

        public ClientDataProvider(ApiClient apiClient, HolidaysProvider holidaysProvider)
        {
            if (holidaysProvider is null)
                throw new ArgumentNullException(nameof(holidaysProvider));

            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

            holidays = new Lazy<HashSet<LocalDate>>(holidaysProvider.GetHolidays);
        }

        public DraftOrder DraftOrder { get; private set; } = new DraftOrder();

        public async Task<IReadOnlyDictionary<int, Resource>> GetResources()
        {
            if (cachedResources == null)
            {
                var (response, problem) = await apiClient.Get<IEnumerable<Resource>>("resources");
                cachedResources = problem == null
                    ? response.ToDictionary(resource => resource.Id)
                    : new Dictionary<int, Resource>();
            }

            return cachedResources;
        }

        public bool IsHighPriceDay(LocalDate date) => HighPricePolicy.IsHighPriceDay(date, holidays.Value);

        public int GetNumberOfHighPriceDays(LocalDate date, int durationInDays)
        {
            return Enumerable.Range(0, durationInDays).Aggregate(
                0,
                (count, i) => count + (HighPricePolicy.IsHighPriceDay(date.PlusDays(i), holidays.Value) ? 1 : 0));
        }

        public async Task<IEnumerable<ReservedDay>> GetReservedDays(
            int resourceId, LocalDate fromDate, LocalDate toDate, bool includeOrder)
            => (await GetReservedDays(includeOrder)).Where(
                day => day.ResourceId == resourceId && fromDate <= day.Date && day.Date <= toDate);

        public void Refresh() => cachedReservedDays = null;

        public async Task<IEnumerable<WeeklyLockBoxCodes>> GetWeeklyLockBoxCodes()
        {
            if (cachedWeeklyLockBoxCodes == null)
            {
                var (response, problem) = await apiClient.Get<IEnumerable<WeeklyLockBoxCodes>>("lock-box-codes");
                cachedWeeklyLockBoxCodes = problem == null ? response! : Enumerable.Empty<WeeklyLockBoxCodes>();
            }

            return cachedWeeklyLockBoxCodes;
        }

        public void ResetState()
        {
            Refresh();
            DraftOrder = new DraftOrder();
        }

        public async Task<IEnumerable<Apartment>> GetApartments()
        {
            if (cachedApartments is null)
            {
                var (response, problem) = await apiClient.Get<IEnumerable<Apartment>>("apartments");
                cachedApartments = problem == null ? response! : Enumerable.Empty<Apartment>();
            }

            return cachedApartments;
        }

        public async Task<IEnumerable<ReservedDay>> GetReservedDays(
            LocalDate fromDate, LocalDate toDate, bool includeOrder)
            => (await GetReservedDays(includeOrder)).Where(day => fromDate <= day.Date && day.Date <= toDate);

        private async Task<IEnumerable<ReservedDay>> GetReservedDays(bool includeOrder)
        {
            if (cachedReservedDays == null)
            {
                const string requestUri = "reserved-days";
                var (response, problem) = await apiClient.Get<IEnumerable<ReservedDay>>(requestUri);
                cachedReservedDays = problem == null ? response! : Enumerable.Empty<ReservedDay>();
            }

            if (!includeOrder)
                return cachedReservedDays;

            var reservations = cachedReservedDays!.Concat(GetOrderReservedDays());
            if (DraftOrder.DraftReservation != null)
                reservations = reservations.Concat(GetReservationReservedDays(DraftOrder.DraftReservation));
            return reservations;
        }

        private IEnumerable<ReservedDay> GetOrderReservedDays()
            => DraftOrder.Reservations.SelectMany(GetReservationReservedDays);

        private static IEnumerable<ReservedDay> GetReservationReservedDays(DraftReservation reservation)
            => Enumerable.Range(0, reservation.DurationInDays).Select(
                i => new ReservedDay
                {
                    Date = reservation.Date.PlusDays(i), ResourceId = reservation.Resource.Id, IsMyReservation = true
                });
    }
}