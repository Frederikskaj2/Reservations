using Frederikskaj2.Reservations.Core;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Client;

class DateProvider : IDateProvider
{
    readonly IClock clock;
    readonly ITimeConverter timeConverter;

    public DateProvider(IClock clock, ITimeConverter timeConverter)
    {
        this.clock = clock;
        this.timeConverter = timeConverter;
        Holidays = HolidaysProvider.Get(Today);
    }

    public Instant Now => clock.GetCurrentInstant();

    public LocalDate Today => timeConverter.GetDate(Now);

    public IReadOnlySet<LocalDate> Holidays { get; }
}
