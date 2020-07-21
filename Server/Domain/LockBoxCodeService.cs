using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Server.Email;
using Frederikskaj2.Reservations.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Calendars;
using Resource = Frederikskaj2.Reservations.Server.Data.Resource;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Domain
{
    public class LockBoxCodeService
    {
        private const int LockBoxCodeCount = 15;
        private const int MaximumDigits = 6;
        private const int MinimumDigits = 4;

        private static readonly int[] AllDigits = Enumerable.Range(0, 10).ToArray();
        private static readonly Random Random = new Random();

        private readonly IBackgroundWorkQueue<IEmailService> backgroundWorkQueue;
        private readonly ReservationsContext db;
        private readonly ILogger<LockBoxCodeService> logger;
        private readonly ReservationsOptions reservationsOptions;

        public LockBoxCodeService(
            ILogger<LockBoxCodeService> logger, IBackgroundWorkQueue<IEmailService> backgroundWorkQueue,
            ReservationsContext db, ReservationsOptions reservationsOptions)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.backgroundWorkQueue = backgroundWorkQueue ?? throw new ArgumentNullException(nameof(backgroundWorkQueue));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.reservationsOptions = reservationsOptions ?? throw new ArgumentNullException(nameof(reservationsOptions));
        }

        public async Task<IEnumerable<Data.LockBoxCode>> GetLockBoxCodes(LocalDate date)
        {
            var daysAfterMonday = ((int) date.DayOfWeek - 1)%7;
            var latestMonday = date.PlusDays(-daysAfterMonday).PlusWeeks(-1);

            var resources = await db.Resources
                .Include(resource => resource.LockBoxCodes)
                .ToListAsync();
            var allLockBoxCodes = resources
                .SelectMany(resource => resource.LockBoxCodes.Select(code => new LockBoxCode(code.Code)))
                .ToHashSet();
            foreach (var resource in resources)
            {
                var codeForLatestMonday = resource.LockBoxCodes.FirstOrDefault(code => code.Date == latestMonday);
                if (codeForLatestMonday == null)
                {
                    var previousCode = GetPreviousLockBoxCode(resource);
                    codeForLatestMonday = new Data.LockBoxCode
                    {
                        ResourceId = resource.Id,
                        Date = latestMonday,
                        Code = GenerateNextLockBoxCode(previousCode).ToString()
                    };
                    resource.LockBoxCodes!.Add(codeForLatestMonday);
                }

                var oldCodes = resource.LockBoxCodes.Where(code => code.Date < latestMonday).ToList();
                foreach (var code in oldCodes)
                    if (code.Date < latestMonday)
                        resource.LockBoxCodes!.Remove(code);

                var mostFutureMonday = latestMonday.PlusWeeks(LockBoxCodeCount - 1);
                var latestCode = codeForLatestMonday;
                while (latestCode.Date < mostFutureMonday)
                {
                    var nextMonday = latestCode.Date.PlusWeeks(1);
                    var nextCode = resource.LockBoxCodes.FirstOrDefault(code => code.Date == nextMonday);
                    if (nextCode == null)
                    {
                        nextCode = new Data.LockBoxCode
                        {
                            ResourceId = resource.Id,
                            Date = nextMonday,
                            Code = GenerateNextLockBoxCode(new LockBoxCode(latestCode.Code)).ToString()
                        };
                        resource.LockBoxCodes!.Add(nextCode);
                    }
                    latestCode = nextCode;
                }
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                logger.LogWarning(exception, "Error updating lock box codes.");
                return Enumerable.Empty<Data.LockBoxCode>();
            }

            return resources.SelectMany(resource => resource.LockBoxCodes.Take(LockBoxCodeCount));

            LockBoxCode GetPreviousLockBoxCode(Resource resource)
            {
                var previousCode = resource.LockBoxCodes
                    .Where(code => code.Date < latestMonday)
                    .OrderByDescending(code => code.Date)
                    .FirstOrDefault();
                return previousCode != null ? new LockBoxCode(previousCode.Code) : GenerateRandomLockBoxCode();
            }

            LockBoxCode GenerateRandomLockBoxCode()
            {
                while (true)
                {
                    var code = GenerateRandomLockBoxCodeCore();
                    if (allLockBoxCodes.Contains(code))
                        continue;
                    allLockBoxCodes.Add(code);
                    return code;
                }
            }

            static LockBoxCode GenerateRandomLockBoxCodeCore()
            {
                var length = Random.Next(MinimumDigits, MaximumDigits + 1);
                var digits = (int[]) AllDigits.Clone();
                return new LockBoxCode(MemoryMarshal.ToEnumerable<int>(digits.AsMemory().Shuffle(length, Random)));
            }

            LockBoxCode GenerateNextLockBoxCode(LockBoxCode code)
            {
                while (true)
                {
                    var nextCode = GenerateNextLockBoxCodeCore(code);
                    if (allLockBoxCodes.Contains(nextCode))
                        continue;
                    allLockBoxCodes.Add(nextCode);
                    return nextCode;
                }
            }

            static LockBoxCode GenerateNextLockBoxCodeCore(LockBoxCode code)
            {
                var digitsToRemoveCount = Random.Next(1, 3);
                var digitsToAddCount = Random.Next(1, 3);
                if (code.DigitCount - digitsToRemoveCount + digitsToAddCount < MinimumDigits - 1)
                {
                    digitsToAddCount = MinimumDigits + digitsToRemoveCount - code.DigitCount;
                }
                else if (code.DigitCount - digitsToRemoveCount + digitsToAddCount < MinimumDigits)
                {
                    if (Random.Next(0, 1) == 0)
                        digitsToRemoveCount -= 1;
                    else
                        digitsToAddCount += 1;
                }
                else if (code.DigitCount - digitsToRemoveCount + digitsToAddCount > MaximumDigits + 1)
                {
                    digitsToRemoveCount = -MaximumDigits + digitsToAddCount + code.DigitCount;
                }
                else if (code.DigitCount - digitsToRemoveCount + digitsToAddCount > MaximumDigits)
                {
                    if (Random.Next(0, 1) == 0)
                        digitsToAddCount -= 1;
                    else
                        digitsToRemoveCount += 1;
                }
                if (digitsToRemoveCount + digitsToAddCount >= 4 && digitsToRemoveCount >= 1 && digitsToAddCount >= 1)
                {
                    digitsToRemoveCount -= 1;
                    digitsToAddCount -= 1;
                }

                var digitsToRemove = digitsToRemoveCount > 0 ? code.ToMemory().Shuffle(digitsToRemoveCount, Random) : Array.Empty<int>();
                var digitsToAdd = digitsToAddCount > 0 ? code.Inverse().ToMemory().Shuffle(digitsToAddCount, Random) : Array.Empty<int>();
                foreach (var digit in digitsToRemove.Span)
                    code = code.RemoveDigit(digit);
                foreach (var digit in digitsToAdd.Span)
                    code = code.AddDigit(digit);
                return code;
            }
        }

        public async Task<IEnumerable<WeeklyLockBoxCodes>> GetWeeklyLockBoxCodes(LocalDate date)
        {
            var weeklyLockBoxCodes = (await GetLockBoxCodes(date))
                .GroupBy(code => code.Date)
                .Select(
                    grouping => new WeeklyLockBoxCodes
                    {
                        WeekNumber = WeekYearRules.Iso.GetWeekOfWeekYear(grouping.Key),
                        Date = grouping.Key,
                        Codes = grouping
                            .Select(code => new Shared.LockBoxCode { ResourceId = code.ResourceId, Code = code.Code })
                            .ToList()
                    })
                .OrderBy(code => code.WeekNumber)
                .ToList();

            for (var i = 1; i < weeklyLockBoxCodes.Count; i += 1)
            {
                var previousWeek = weeklyLockBoxCodes[i - 1];
                var thisWeek = weeklyLockBoxCodes[i];
                foreach (var code in thisWeek.Codes!)
                {
                    var previousWeekCode = previousWeek.Codes.First(c => c.ResourceId == code.ResourceId);
                    var difference = LockBoxCode.GetDifference(new LockBoxCode(code.Code), new LockBoxCode(previousWeekCode.Code));
                    code.Difference = difference.ToString();
                }
            }

            return weeklyLockBoxCodes;
        }

        public async Task SendLockBoxCodeEmails(LocalDate date)
        {
            var reservations = (await db.Reservations
                .Include(reservation => reservation.Order)
                .ThenInclude(order => order!.User)
                .Include(reservation => reservation.Resource)
                .Where(reservation =>
                    !reservation.Order!.Flags.HasFlag(OrderFlags.IsOwnerOrder)
                    && reservation.Status == ReservationStatus.Confirmed
                    && !reservation.SentEmails.HasFlag(ReservationEmails.LockBoxCode))
                .ToListAsync())
                .Where(
                    reservation => reservation.Date.PlusDays(
                        -reservationsOptions.RevealLockBoxCodeDaysBeforeReservationStart) <= date)
                .ToList();

            if (reservations.Count == 0)
                return;

            var lockBoxCodes = await GetLockBoxCodes(date);
            foreach (var reservation in reservations)
            {
                backgroundWorkQueue.Enqueue((service, _) => service.SendLockBoxCodeEmail(reservation.Order!.User!, reservation, lockBoxCodes));
                reservation.SentEmails |= ReservationEmails.LockBoxCode;
            }

            await db.SaveChangesAsync();
        }

        public async Task SendWeeklyLockBoxCodes(User user, LocalDate date)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var weeklyLockBoxCodes = await GetWeeklyLockBoxCodes(date);

            var resources = await db.Resources.ToListAsync();
            backgroundWorkQueue.Enqueue(
                (service, _) => service.SendWeeklyLockBoxCodesEmail(user, resources, weeklyLockBoxCodes));
        }
    }
}