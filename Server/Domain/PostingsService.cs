using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Server.Email;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Posting = Frederikskaj2.Reservations.Shared.Posting;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Domain
{
    public class PostingsService
    {
        private readonly IBackgroundWorkQueue<IEmailService> backgroundWorkQueue;
        private readonly IClock clock;
        private readonly DateTimeZone dateTimeZone;
        private readonly ReservationsContext db;

        public PostingsService(IBackgroundWorkQueue<IEmailService> backgroundWorkQueue, IClock clock,
            DateTimeZone dateTimeZone, ReservationsContext db)
        {
            this.backgroundWorkQueue = backgroundWorkQueue ?? throw new ArgumentNullException(nameof(backgroundWorkQueue));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<PostingsRange> GetPostingsRange()
        {
            var earliestPosting = await db.Postings.OrderBy(posting => posting.Date).FirstOrDefaultAsync();

            if (earliestPosting == null)
            {
                var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
                var monthStart = GetMonthStart(today);
                return new PostingsRange
                {
                    EarliestMonth = monthStart,
                    LatestMonth = monthStart
                };
            }

            var latestPosting = await db.Postings.OrderByDescending(posting => posting.Date).FirstOrDefaultAsync();
            return new PostingsRange
            {
                EarliestMonth = GetMonthStart(earliestPosting.Date),
                LatestMonth = GetMonthStart(latestPosting.Date)
            };
        }

        public async Task<IEnumerable<Posting>> GetPostings(LocalDate month)
        {
            var fromDate = GetMonthStart(month);
            var toDate = GetMonthStart(month).PlusMonths(1);
            return await db.Postings
                .Where(posting => fromDate <= posting.Date && posting.Date < toDate)
                .ProjectToType<Posting>()
                .ToListAsync();
        }

        public async Task SendPostingsEmail(User user, LocalDate date)
        {
            var postings = await GetPostings(date);
            backgroundWorkQueue.Enqueue((service, _) => service.SendPostingsEmail(user, postings));
        }

        private static LocalDate GetMonthStart(LocalDate date) => date.PlusDays(-(date.Day - 1));
    }
}
