using NodaTime;
using NodaTime.TimeZones;

namespace Frederikskaj2.Reservations.Core;

class TimeConverter(DateTimeZone dateTimeZone) : ITimeConverter
{
    public LocalDate GetDate(Instant instant) => instant.InZone(dateTimeZone).Date;

    public LocalDateTime GetTime(Instant instant) => instant.InZone(dateTimeZone).LocalDateTime;

    public Instant GetMidnight(LocalDate date) => date.AtMidnight().InZone(dateTimeZone, Resolvers.LenientResolver).ToInstant();

    public Instant GetInstant(LocalDateTime time) => time.InZone(dateTimeZone, Resolvers.LenientResolver).ToInstant();
}
