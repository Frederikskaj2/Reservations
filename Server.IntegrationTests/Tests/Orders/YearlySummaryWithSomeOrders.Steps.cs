using FluentAssertions;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using Frederikskaj2.Reservations.Users;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using NodaTime;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class YearlySummaryWithSomeOrders(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    State<IReadOnlyList<MyOrderDto>> orders;
    State<YearlySummaryRange> yearlySummaryRange;
    State<YearlySummary> yearlySummary;

    YearlySummaryRange YearlySummaryRange => yearlySummaryRange.GetValue(nameof(YearlySummaryRange));
    YearlySummary YearlySummary => yearlySummary.GetValue(nameof(YearlySummary));
    IReadOnlyList<MyOrderDto> Orders => orders.GetValue(nameof(Orders));

    async Task IScenarioSetUp.OnScenarioSetUp()
    {
        session.NowOffset = Period.Zero;
        await session.UpdateLockBoxCodes();
    }

    async Task GivenAFewOrders()
    {
        await session.SignUpAndSignIn();
        var placeResidentOrderResponse1 = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        var placeResidentOrderResponse2 = await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        var placeResidentOrderResponse3 = await session.PlaceAndPayResidentOrder(
            new TestReservation(SeedData.BanquetFacilities.ResourceId, 2),
            new TestReservation(SeedData.Frederik.ResourceId));
        orders = new([placeResidentOrderResponse1.Order, placeResidentOrderResponse2.Order, placeResidentOrderResponse3.Order]);
        session.NowOffset = Period.FromDays(365);
        await session.PlaceAndPayResidentOrder(new TestReservation(SeedData.Frederik.ResourceId));
        await session.ConfirmOrders();
    }

    async Task WhenTheYearlySummaryRangeIsRetrieved()
    {
        var getYearlySummaryRangeResponse = await session.GetYearlySummaryRange();
        yearlySummaryRange = getYearlySummaryRangeResponse.YearlySummaryRange;
    }

    async Task WhenTheYearlySummaryIsRetrieved()
    {
        var getYearlySummaryResponse = await session.GetYearlySummary(YearlySummaryRange.EarliestYear);
        yearlySummary = getYearlySummaryResponse.YearlySummary;
    }

    Task ThenTheYearRangeIsTwoYears()
    {
        YearlySummaryRange.EarliestYear.Should().Be(session.CurrentDate.Year - 1);
        YearlySummaryRange.LatestYear.Should().Be(session.CurrentDate.Year);
        return Task.CompletedTask;
    }

    Task ThenTheYearlySummaryIsNotEmpty()
    {
        var year = session.CurrentDate.Year - 1;
        YearlySummary.Year.Should().Be(year);
        var expected = Orders
            .SelectMany(order => order.Reservations)
            .Where(reservation => reservation.Extent.Date.Year == year)
            .GroupBy(reservation => GetResourceType(reservation.ResourceId))
            .Select(grouping =>
                grouping
                    .Aggregate(new ResourceSummary(grouping.Key, 0, 0, Amount.Zero),
                        (sum, reservation) => sum with
                        {
                            ReservationCount = sum.ReservationCount + 1,
                            Nights = sum.Nights + reservation.Extent.Nights,
                            Income = sum.Income + reservation.Price!.Rent + reservation.Price.Cleaning,
                        }));
        YearlySummary.ResourceSummaries.Should().BeEquivalentTo(expected);
        return Task.CompletedTask;
    }

    static ResourceType GetResourceType(ResourceId resourceId) =>
        resourceId == SeedData.BanquetFacilities.ResourceId
            ? ResourceType.BanquetFacilities
            : resourceId == SeedData.Frederik.ResourceId || resourceId == SeedData.Kaj.ResourceId
                ? ResourceType.Bedroom
                : throw new UnreachableException();
}
