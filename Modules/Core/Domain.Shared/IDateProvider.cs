using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Core;

public interface IDateProvider
{
    Instant Now { get; }
    LocalDate Today { get; }
    IReadOnlySet<LocalDate> Holidays { get; }
}
