using FluentAssertions;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Framework;
using LightBDD.XUnit2;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Orders;

sealed partial class YearlySummaryWithNoOrders(SessionFixture session) : FeatureFixture, IClassFixture<SessionFixture>
{
    State<YearlySummaryRange> yearlySummaryRange;
    State<YearlySummary> yearlySummary;

    YearlySummaryRange YearlySummaryRange => yearlySummaryRange.GetValue(nameof(YearlySummaryRange));
    YearlySummary YearlySummary => yearlySummary.GetValue(nameof(YearlySummary));

    static Task GivenNoOrders() => Task.CompletedTask;

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

    Task ThenTheYearRangeIsThisYear()
    {
        YearlySummaryRange.EarliestYear.Should().Be(session.CurrentDate.Year);
        YearlySummaryRange.LatestYear.Should().Be(session.CurrentDate.Year);
        return Task.CompletedTask;
    }

    Task ThenTheYearlySummaryIsEmpty()
    {
        YearlySummary.Year.Should().Be(session.CurrentDate.Year);
        YearlySummary.ResourceSummaries.Should().BeEmpty();
        return Task.CompletedTask;
    }
}
