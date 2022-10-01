using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Core;

public class DateProviderOptions
{
    public Period NowOffset { get; set; } = Period.Zero;
}
