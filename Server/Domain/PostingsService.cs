using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Server.Email;
using Frederikskaj2.Reservations.Shared;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Posting = Frederikskaj2.Reservations.Shared.Posting;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Domain
{
    public class PostingsService
    {
        private readonly IBackgroundWorkQueue<EmailService> backgroundWorkQueue;
        private readonly IClock clock;
        private readonly DateTimeZone dateTimeZone;
        private readonly ReservationsContext db;

        public PostingsService(IBackgroundWorkQueue<EmailService> backgroundWorkQueue, IClock clock,
            DateTimeZone dateTimeZone, ReservationsContext db)
        {
            this.backgroundWorkQueue = backgroundWorkQueue ?? throw new ArgumentNullException(nameof(backgroundWorkQueue));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<PostingsRange> GetPostingsRange()
        {
            var earliestTransaction = await db.Transactions.OrderBy(transaction => transaction.Timestamp).FirstOrDefaultAsync();

            if (earliestTransaction == null)
            {
                var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
                var monthStart = GetMonthStart(today);
                return new PostingsRange
                {
                    EarliestMonth = monthStart,
                    LatestMonth = monthStart
                };
            }

            var latestTransaction = await db.Transactions.OrderByDescending(transaction => transaction.Date).FirstOrDefaultAsync();
            return new PostingsRange
            {
                EarliestMonth = GetMonthStart(earliestTransaction.Date),
                LatestMonth = GetMonthStart(latestTransaction.Date)
            };
        }

        public async Task<IEnumerable<Posting>> GetPostings(LocalDate month)
        {
            // Unfortunately, getting the required transactions require two round-trips to the database.
            var fromTimestamp = GetMonthStart(month);
            var toTimestamp = GetMonthStart(month).PlusMonths(1);
            var orderIds = await db.Transactions
                .Where(
                    transaction
                        => (transaction.Type == TransactionType.PayIn || transaction.Type == TransactionType.PayOut)
                        && fromTimestamp <= transaction.Date && transaction.Date < toTimestamp)
                .Select(transaction => transaction.OrderId)
                .Distinct()
                .ToListAsync();

            var transactions = await db.Transactions
                .Where(transaction => orderIds.Contains(transaction.OrderId))
                .ToListAsync();

            return transactions
                .GroupBy(transaction => transaction.OrderId)
                .SelectMany(CreatePostings)
                .OrderBy(posting => posting.Date)
                .ThenBy(posting => posting.OrderId);

            static IEnumerable<Posting> CreatePostings(IGrouping<int, Transaction> grouping)
            {
                var orderState = new OrderState(grouping.Key);
                foreach (var transaction in grouping.OrderBy(t => t.Timestamp))
                    orderState.ApplyTransaction(transaction);
                return orderState.Postings;
            }
        }

        public async Task SendPostingsEmail(User user, LocalDate date)
        {
            var postings = await GetPostings(date);
            backgroundWorkQueue.Enqueue((service, _) => service.SendPostingsEmail(user, postings));
        }

        private static LocalDate GetMonthStart(LocalDate date) => date.PlusDays(-(date.Day - 1));
    }
}
