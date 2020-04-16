using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Server.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using Resource = Frederikskaj2.Reservations.Server.Data.Resource;

namespace Frederikskaj2.Reservations.Server.Domain
{
    public class KeyCodeService
    {
        private const int KeyCodeLength = 5;
        private const int KeyCodeCount = 14;
        private static readonly Random Random = new Random();
        private readonly IBackgroundWorkQueue<EmailService> backgroundWorkQueue;
        private readonly ReservationsContext db;
        private readonly ILogger<KeyCodeService> logger;

        public KeyCodeService(
            ILogger<KeyCodeService> logger, IBackgroundWorkQueue<EmailService> backgroundWorkQueue,
            ReservationsContext db)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.backgroundWorkQueue = backgroundWorkQueue ?? throw new ArgumentNullException(nameof(backgroundWorkQueue));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IEnumerable<KeyCode>> GetKeyCodes(LocalDate date)
        {
            var daysAfterMonday = ((int) date.DayOfWeek - 1)%7;
            var latestMonday = date.PlusDays(-daysAfterMonday);

            var resources = await db.Resources
                .Include(resource => resource.KeyCodes)
                .ToListAsync();
            var allKeyCodes = resources
                .SelectMany(resource => resource.KeyCodes.Select(keyCode => keyCode.Code))
                .ToHashSet();
            foreach (var resource in resources)
            {
                var keyCodeForLatestMonday = resource.KeyCodes.FirstOrDefault(keyCode => keyCode.Date == latestMonday);
                if (keyCodeForLatestMonday == null)
                {
                    var previousKeyCode = GetPreviousKeyCode(resource);
                    keyCodeForLatestMonday = new KeyCode
                    {
                        ResourceId = resource.Id,
                        Date = latestMonday,
                        Code = GenerateNextKeyCode(previousKeyCode)
                    };
                    resource.KeyCodes!.Add(keyCodeForLatestMonday);
                }

                var oldKeyCodes = resource.KeyCodes.Where(keyCode => keyCode.Date < latestMonday).ToList();
                foreach (var keyCode in oldKeyCodes)
                    if (keyCode.Date < latestMonday)
                        resource.KeyCodes!.Remove(keyCode);

                var mostFutureMonday = latestMonday.PlusWeeks(KeyCodeCount - 1);
                var latestKeyCode = keyCodeForLatestMonday;
                while (latestKeyCode.Date < mostFutureMonday)
                {
                    var nextMonday = latestKeyCode.Date.PlusWeeks(1);
                    var nextKeyCode = resource.KeyCodes.FirstOrDefault(keyCode => keyCode.Date == nextMonday);
                    if (nextKeyCode == null)
                    {
                        nextKeyCode = new KeyCode
                        {
                            ResourceId = resource.Id,
                            Date = nextMonday,
                            Code = GenerateNextKeyCode(latestKeyCode.Code)
                        };
                        resource.KeyCodes!.Add(nextKeyCode);
                    }
                    latestKeyCode = nextKeyCode;
                }
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                logger.LogWarning(exception, "Error updating key codes.");
                return Enumerable.Empty<KeyCode>();
            }

            return resources.SelectMany(resource => resource.KeyCodes.OrderBy(keyCode => keyCode.Date).Take(KeyCodeCount));

            string GetPreviousKeyCode(Resource resource)
            {
                var previousKeyCode = resource.KeyCodes
                    .Where(keyCode => keyCode.Date < latestMonday)
                    .OrderByDescending(keyCode => keyCode.Date)
                    .FirstOrDefault();
                return previousKeyCode != null ? previousKeyCode.Code : GenerateRandomKeyCode();
            }

            string GenerateRandomKeyCode()
            {
                while (true)
                {
                    var code = GenerateRandomKeyCodeCore(KeyCodeLength);
                    if (allKeyCodes.Contains(code))
                        continue;
                    allKeyCodes.Add(code);
                    return code;
                }
            }

            string GenerateNextKeyCode(string code)
            {
                while (true)
                {
                    var nextCode = GenerateNextKeyCodeCore(code);
                    if (allKeyCodes.Contains(nextCode))
                        continue;
                    allKeyCodes.Add(nextCode);
                    return nextCode;
                }
            }
        }

        public async Task SendKeyCodes(User user, LocalDate date)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var keyCodes = await GetKeyCodes(date);

            var resources = await db.Resources.ToListAsync();
            backgroundWorkQueue.Enqueue(
                (service, _) => service.SendKeyCodesEmail(user, resources, keyCodes));
        }

        private static string GenerateNextKeyCodeCore(string code)
        {
            var index = Random.Next(code.Length);
            var offset = GetOffset();

            var digit = code[index];
            var number = digit - '1' + offset + 9;
            var nextDigit = (char) (number%9 + '1');
            return string.Create(code.Length, (code, index, nextDigit), SpanAction);

            static int GetOffset() => Random.Next(4) switch
            {
                0 => -2,
                1 => -1,
                2 => 1,
                _ => 2
            };

            static void SpanAction(Span<char> span, (string Value, int Index, char Digit) tuple)
            {
                tuple.Value.AsSpan().CopyTo(span);
                span[tuple.Index] = tuple.Digit;
            }
        }

        private static string GenerateRandomKeyCodeCore(int length)
        {
            return string.Create(length, Random, SpanAction);

            static void SpanAction(Span<char> span, Random random)
            {
                for (var i = 0; i < span.Length; i += 1)
                    span[i] = (char) (random.Next(9) + '1');
            }
        }
    }
}