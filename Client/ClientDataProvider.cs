using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using NodaTime;
using NodaTime.Calendars;
using static System.FormattableString;

namespace Frederikskaj2.Reservations.Client
{
    public class ClientDataProvider : IDataProvider
    {
        private readonly ApiClient apiClient;
        private IEnumerable<Apartment>? cachedApartments;
        private HashSet<LocalDate>? cachedHolidays;
        private IEnumerable<ReservedDay>? cachedReservedDays;
        private IReadOnlyDictionary<int, Resource>? cachedResources;
        private IEnumerable<WeeklyKeyCodes>? cachedWeeklyKeyCodes;

        public ClientDataProvider(ApiClient apiClient)
            => this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

        public DraftOrder DraftOrder { get; private set; } = new DraftOrder();

        public async Task<IReadOnlyDictionary<int, Resource>> GetResources()
        {
            if (cachedResources == null)
            {
                var maybe = await apiClient.GetJsonAsync<IEnumerable<Resource>>("resources");
                cachedResources = maybe.TryGetValue(out var resources)
                    ? resources.ToDictionary(resource => resource.Id)
                    : new Dictionary<int, Resource>();
            }

            return cachedResources;
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

        public async Task<IEnumerable<ReservedDay>> GetReservedDays(
            int resourceId, LocalDate fromDate, LocalDate toDate, bool includeOrder)
            => (await GetReservedDays(includeOrder)).Where(
                day => day.ResourceId == resourceId && fromDate <= day.Date && day.Date <= toDate);

        public void Refresh()
        {
            cachedHolidays = null;
            cachedReservedDays = null;
        }

        public async Task<IEnumerable<WeeklyKeyCodes>> GetKeyCodes()
        {
            if (cachedWeeklyKeyCodes == null)
            {
                var maybe = await apiClient.GetJsonAsync<IEnumerable<KeyCode>>("key-codes");
                if (maybe.TryGetValue(out var keyCodes))
                    cachedWeeklyKeyCodes = keyCodes
                        .GroupBy(keyCode => keyCode.Date)
                        .Select(
                            grouping => new WeeklyKeyCodes(
                                WeekYearRules.Iso.GetWeekOfWeekYear(grouping.Key),
                                grouping.Key,
                                grouping.ToDictionary(keyCode => keyCode.ResourceId, keyCode => keyCode.Code)
                            ))
                        .OrderBy(keyCode => keyCode.WeekNumber);
                else
                    cachedWeeklyKeyCodes = Enumerable.Empty<WeeklyKeyCodes>();
            }

            return cachedWeeklyKeyCodes;
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
                var maybe = await apiClient.GetJsonAsync<IEnumerable<Apartment>>("apartments");
                if (!maybe.TryGetValue(out cachedApartments))
                    cachedApartments = Enumerable.Empty<Apartment>();
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
                var requestUri = Invariant($"reserved-days");
                var maybe = await apiClient.GetJsonAsync<IEnumerable<ReservedDay>>(requestUri);
                if (!maybe.TryGetValue(out cachedReservedDays))
                    cachedReservedDays = Enumerable.Empty<ReservedDay>();
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