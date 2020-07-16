using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    [SuppressMessage("Microsoft.Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "This class is instantiated vis dependency injection.")]
    internal class ServerDataProvider : IDataProvider
    {
        private readonly ReservationsContext db;
        private readonly Lazy<HashSet<LocalDate>> holidays;

        public ServerDataProvider(ReservationsContext db, HolidaysProvider holidaysProvider)
        {
            if (holidaysProvider is null)
                throw new ArgumentNullException(nameof(holidaysProvider));

            this.db = db ?? throw new ArgumentNullException(nameof(db));

            holidays = new Lazy<HashSet<LocalDate>>(holidaysProvider.GetHolidays);
        }

        public async Task<IReadOnlyDictionary<int, Shared.Resource>> GetResources()
            => await db.Resources.ToDictionaryAsync(resource => resource.Id, resource => resource.Adapt<Shared.Resource>());

        public bool IsHighPriceDay(LocalDate date) => HighPricePolicy.IsHighPriceDay(date, holidays.Value);

        public int GetNumberOfHighPriceDays(LocalDate date, int durationInDays)
        {
            return Enumerable.Range(0, durationInDays)
                .Aggregate(0, (count, i) => count + (IsHighPriceDay(date.PlusDays(i)) ? 1 : 0));

            bool IsHighPriceDay(LocalDate d) => IsHighPriceWeekDay(d) || holidays.Value.Contains(d);
        }

        public async Task<IEnumerable<Shared.ReservedDay>> GetReservedDays(
            int resourceId, LocalDate fromDate, LocalDate toDate, bool includeOrder)
            => await db.ReservedDays
                .Where(day => day.ResourceId == resourceId && fromDate <= day.Date && day.Date <= toDate)
                .OrderBy(day => day.Date)
                .ProjectToType<Shared.ReservedDay>()
                .ToListAsync();

        private static bool IsHighPriceWeekDay(LocalDate date)
            => date.DayOfWeek == IsoDayOfWeek.Friday
               || date.DayOfWeek == IsoDayOfWeek.Saturday
               || date.DayOfWeek == IsoDayOfWeek.Sunday;
    }
}