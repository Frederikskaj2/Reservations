using Frederikskaj2.Reservations.LockBox;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

record TestReservation(
    ResourceId ResourceId,
    int Nights = 1,
    PriceGroup PriceGroup = default,
    TestReservationType Type = default,
    int AdditionalDaysInTheFuture = 0)
{
    public TestReservation(ResourceId resourceId, int nights, PriceGroup priceGroup)
        : this(resourceId, nights, priceGroup, default) { }

    public TestReservation(ResourceId resourceId, int nights, TestReservationType type)
        : this(resourceId, nights, default, type) { }

    public TestReservation(ResourceId resourceId, int nights, int additionalDaysInTheFuture)
        : this(resourceId, nights, default, default, additionalDaysInTheFuture) { }
}
