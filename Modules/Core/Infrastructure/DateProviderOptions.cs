using NodaTime;

namespace Frederikskaj2.Reservations.Core;

public class DateProviderOptions
{
    public Period NowOffset { get; set; } = Period.Zero;
}
