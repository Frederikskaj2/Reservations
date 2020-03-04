using System;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    internal class DateProvider : IDateProvider
    {
        private readonly IClock clock;
        private readonly DateTimeZone dateTimeZone;

        public DateProvider(IClock clock, DateTimeZone dateTimeZone)
        {
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
        }

        public LocalDate Today => clock.GetCurrentInstant().InZone(dateTimeZone).Date;
    }
}