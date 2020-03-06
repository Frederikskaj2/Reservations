using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Domain
{
    internal class ServerDataProvider : IDataProvider
    {
        private readonly ReservationsContext db;

        public ServerDataProvider(ReservationsContext db)
            => this.db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<bool> IsHighPriceDay(LocalDate date)
            => await HighPricePolicy.IsHighPriceDay(
                date, async () => (await db.Holidays.Select(h => h.Date).ToListAsync()).ToHashSet());

        public async Task<int> GetNumberOfHighPriceDays(Reservation reservation)
        {
            var holidays = (await db.Holidays.Select(h => h.Date).ToListAsync()).ToHashSet();
            return Enumerable.Range(0, reservation.DurationInDays)
                .Aggregate(0, (count, i) => count + (IsHighPriceDay(reservation.Date.PlusDays(i)) ? 1 : 0));

            bool IsHighPriceDay(LocalDate date) => IsHighPriceWeekDay(date) || holidays.Contains(date);
        }

        public async Task<IEnumerable<Reservation>> GetReservations(
            int resourceId, LocalDate fromDate, LocalDate toDate)
            => await db.Reservations
                .Where(rr => rr.ResourceId == resourceId && fromDate <= rr.Date && rr.Date <= toDate)
                .OrderBy(rr => rr.Date)
                .ToListAsync();

        public void Refresh()
        {
            // Do nothing as this is already a scoped service.
        }

        private static bool IsHighPriceWeekDay(LocalDate date)
            => date.DayOfWeek == IsoDayOfWeek.Friday || date.DayOfWeek == IsoDayOfWeek.Saturday ||
               date.DayOfWeek == IsoDayOfWeek.Sunday;
    }
}