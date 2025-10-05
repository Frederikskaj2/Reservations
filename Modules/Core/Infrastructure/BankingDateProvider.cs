using Microsoft.Extensions.Options;
using NodaTime;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Core;

class BankingDateProvider : IBankingDateProvider
{
    readonly IClock clock;
    readonly Period nowOffset;
    readonly ITimeConverter timeConverter;

    public BankingDateProvider(IClock clock, IOptionsSnapshot<DateProviderOptions> options, ITimeConverter timeConverter)
    {
        this.clock = clock;
        this.timeConverter = timeConverter;
        nowOffset = options.Value.NowOffset;
        var holidays = HolidaysProvider.Get(Today);
        Holidays = holidays.Where(holiday => !holiday.IsOnlyBankHoliday).Select(holiday => holiday.Date).ToHashSet();
        BankHolidays = holidays.Select(holiday => holiday.Date).ToHashSet();
    }

    public Instant Now => clock.GetCurrentInstant() + nowOffset.ToDuration();

    public LocalDate Today => timeConverter.GetDate(Now);

    public IReadOnlySet<LocalDate> Holidays { get; }

    public IReadOnlySet<LocalDate> BankHolidays { get; }
}
