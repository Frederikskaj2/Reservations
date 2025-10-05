using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Core;

public interface IBankingDateProvider : IDateProvider
{
    IReadOnlySet<LocalDate> BankHolidays { get; }
}
