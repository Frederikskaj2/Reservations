using Microsoft.Extensions.Options;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Core;

class DateProvider : IDateProvider
{
    readonly IClock clock;
    readonly Period nowOffset;
    readonly ITimeConverter timeConverter;

    public DateProvider(IClock clock, IOptionsSnapshot<DateProviderOptions> options, ITimeConverter timeConverter)
    {
        this.clock = clock;
        this.timeConverter = timeConverter;
        nowOffset = options.Value.NowOffset;
        Holidays = HolidaysProvider.Get(Today);
    }

    public Instant Now => clock.GetCurrentInstant() + nowOffset.ToDuration();

    public LocalDate Today => timeConverter.GetDate(Now);

    public IReadOnlySet<LocalDate> Holidays { get; }
}
