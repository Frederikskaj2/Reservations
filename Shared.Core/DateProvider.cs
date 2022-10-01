using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.TimeZones;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

class DateProvider : IDateProvider
{
    readonly IClock clock;
    readonly DateTimeZone dateTimeZone;
    readonly Period nowOffset;

    public DateProvider(IClock clock, DateTimeZone dateTimeZone, IOptionsSnapshot<DateProviderOptions> options)
    {
        this.clock = clock;
        this.dateTimeZone = dateTimeZone;
        nowOffset = options.Value.NowOffset;
        Holidays = HolidaysProvider.Get(Today);
    }

    public Instant Now => clock.GetCurrentInstant() + nowOffset.ToDuration();

    public LocalDate Today => GetDate(Now);

    public IReadOnlySet<LocalDate> Holidays { get; }

    public int GetDaysFromToday(LocalDate date) => (date - Today).Days;

    public int GetDaysFromToday(Instant instant) => GetDaysFromToday(instant.InZone(dateTimeZone).Date);

    public LocalDate GetDate(Instant instant) => instant.InZone(dateTimeZone).Date;

    public LocalDateTime GetTime(Instant instant) => instant.InZone(dateTimeZone).LocalDateTime;

    public Instant GetMidnight(LocalDate date) => date.AtMidnight().InZone(dateTimeZone, Resolvers.LenientResolver).ToInstant();
}
