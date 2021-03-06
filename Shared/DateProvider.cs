﻿using System;
using System.Diagnostics.CodeAnalysis;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    [SuppressMessage("Microsoft.Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "This class is instantiated vis dependency injection.")]
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

        public int GetDaysFromToday(LocalDate date) => (date - Today).Days;
        public int GetDaysFromToday(Instant instant) => GetDaysFromToday(instant.InZone(dateTimeZone).Date);
    }
}