using Frederikskaj2.Reservations.Shared.Core;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

[SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "https://github.com/dotnet/roslyn-analyzers/issues/4462")]
public record TestReservation(
    ResourceId ResourceId, int Nights = 1, PriceGroup PriceGroup = default, bool Early = default, int AdditionalDaysInTheFuture = 0)
{
    public TestReservation(ResourceId resourceId, int nights, PriceGroup priceGroup) : this(resourceId, nights, priceGroup, default) { }

    public TestReservation(ResourceId resourceId, int nights, bool early) : this(resourceId, nights, default, early) { }

    public TestReservation(ResourceId resourceId, int nights, int additionalDaysInTheFuture)
        : this(resourceId, nights, default, default, additionalDaysInTheFuture) { }
}
