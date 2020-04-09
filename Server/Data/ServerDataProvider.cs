using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    internal class ServerDataProvider : IDataProvider
    {
        private readonly ReservationsContext db;

        public ServerDataProvider(ReservationsContext db)
            => this.db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<IReadOnlyDictionary<int, Shared.Resource>> GetResources()
            => await db.Resources.ToDictionaryAsync(resource => resource.Id, resource => resource.Adapt<Shared.Resource>());

        public async Task<bool> IsHighPriceDay(LocalDate date)
            => await HighPricePolicy.IsHighPriceDay(
                date, async () => (await db.Holidays.Select(h => h.Date).ToListAsync()).ToHashSet());

        public async Task<int> GetNumberOfHighPriceDays(LocalDate date, int durationInDays)
        {
            var holidays = (await db.Holidays.Select(h => h.Date).ToListAsync()).ToHashSet();
            return Enumerable.Range(0, durationInDays)
                .Aggregate(0, (count, i) => count + (IsHighPriceDay(date.PlusDays(i)) ? 1 : 0));

            bool IsHighPriceDay(LocalDate d) => IsHighPriceWeekDay(d) || holidays.Contains(d);
        }

        public async Task<IEnumerable<Shared.ReservedDay>> GetReservedDays(
            int resourceId, LocalDate fromDate, LocalDate toDate, bool includeOrder)
            => await db.ReservedDays
                .Where(day => day.ResourceId == resourceId && fromDate <= day.Date && day.Date <= toDate)
                .OrderBy(day => day.Date)
                .ProjectToType<Shared.ReservedDay>()
                .ToListAsync();

        public void Refresh()
        {
            // Do nothing as this is already a scoped service.
        }

        private static bool IsHighPriceWeekDay(LocalDate date)
            => date.DayOfWeek == IsoDayOfWeek.Friday
               || date.DayOfWeek == IsoDayOfWeek.Saturday
               || date.DayOfWeek == IsoDayOfWeek.Sunday;
    }
}